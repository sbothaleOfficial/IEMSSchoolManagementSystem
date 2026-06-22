using IEMS.Application.DTOs;
using IEMS.Core.Interfaces;
using IEMS.Core.Services;
using IEMS.Core.Entities;
using IEMS.Core.Configuration;
using IEMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IEMS.Application.Services;

public class BulkPromotionService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IClassRepository _classRepository;
    private readonly IFeePaymentRepository _feePaymentRepository;
    private readonly StudentPromotionService _promotionService;
    private readonly StudentEligibilityValidator _eligibilityValidator;
    private readonly ClassProgressionValidator _progressionValidator;
    private readonly ApplicationDbContext _context;

    public BulkPromotionService(
        IStudentRepository studentRepository,
        IClassRepository classRepository,
        IFeePaymentRepository feePaymentRepository,
        StudentPromotionService promotionService,
        StudentEligibilityValidator eligibilityValidator,
        ClassProgressionValidator progressionValidator,
        ApplicationDbContext context)
    {
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _feePaymentRepository = feePaymentRepository;
        _promotionService = promotionService;
        _eligibilityValidator = eligibilityValidator;
        _progressionValidator = progressionValidator;
        _context = context;
    }

    public async Task<List<StudentPromotionDto>> GetPromotionPreviewAsync(int fromClassId, int toClassId)
    {
        var students = await _studentRepository.GetStudentsWithClassDetailsAsync(fromClassId);
        var fromClass = await _classRepository.GetByIdAsync(fromClassId);
        var toClass = await _classRepository.GetByIdAsync(toClassId);

        var result = new List<StudentPromotionDto>();

        foreach (var student in students)
        {
            // Simplified: All students are eligible for promotion (basic class update)
            result.Add(new StudentPromotionDto
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                StudentNumber = student.StudentNumber,
                CurrentClass = fromClass?.Name ?? "Unknown",
                TargetClass = toClass?.Name ?? "Unknown",
                IsEligible = true, // All students eligible for simple class update
                IneligibilityReason = string.Empty,
                HasPendingFees = false, // Not checking fees for simple promotion
                PendingAmount = 0
            });
        }

        return result;
    }

    public async Task<BulkPromotionResult> ExecuteBulkPromotionAsync(BulkPromotionRequest request)
    {
        var result = new BulkPromotionResult
        {
            PromotionDate = DateTime.UtcNow,
            AcademicYear = request.AcademicYear
        };

        try
        {
            // Get source students
            var allStudents = (await _studentRepository.GetStudentsByClassIdAsync(request.FromClassId)).ToList();

            // Get class details for validation
            var fromClass = await _classRepository.GetByIdAsync(request.FromClassId);
            var toClass = await _classRepository.GetByIdAsync(request.ToClassId);

            if (fromClass == null || toClass == null)
            {
                result.Errors.Add(new PromotionError
                {
                    StudentId = 0,
                    StudentName = "System",
                    Error = "Invalid source or target class"
                });
                return result;
            }

            // Simple validation: just check if classes exist and are different
            if (request.FromClassId == request.ToClassId)
            {
                result.Errors.Add(new PromotionError
                {
                    StudentId = 0,
                    StudentName = "System",
                    Error = "Source and target classes must be different"
                });
                return result;
            }

            // Filter excluded students
            var studentsToPromote = allStudents
                .Where(s => !request.ExcludedStudentIds.Contains(s.Id))
                .ToList();

            result.TotalStudents = studentsToPromote.Count;

            // Simple execution: Just update the ClassId for all students
            if (studentsToPromote.Any())
            {
                try
                {
                    // Get current username for audit (passed from UI layer)
                    var promotedBy = request.PromotedBy ?? "System";

                    // Simple class update - no complex validations
                    foreach (var student in studentsToPromote)
                    {
                        // Save promotion history
                        var promotionHistory = new StudentPromotionHistory
                        {
                            StudentId = student.Id,
                            StudentName = student.FullName,
                            FromClassId = request.FromClassId,
                            FromClassName = fromClass.Name,
                            ToClassId = request.ToClassId,
                            ToClassName = toClass.Name,
                            AcademicYearId = request.AcademicYearId,
                            PromotionDate = DateTime.UtcNow,
                            PromotedBy = promotedBy,
                            Remarks = request.Remarks
                        };
                        _context.StudentPromotionHistory.Add(promotionHistory);

                        // Update student class
                        student.ClassId = request.ToClassId;
                        student.UpdatedAt = DateTime.UtcNow;
                    }

                    await _studentRepository.UpdateMultipleStudentsAsync(studentsToPromote);
                    await _context.SaveChangesAsync(); // Save promotion history
                    result.PromotedStudents = studentsToPromote.Count;
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new PromotionError
                    {
                        StudentId = 0,
                        StudentName = "Database",
                        Error = $"Failed to update students: {ex.Message}"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add(new PromotionError
            {
                StudentId = 0,
                StudentName = "System",
                Error = $"Unexpected error: {ex.Message}"
            });
        }

        return result;
    }

    public async Task<BulkPromotionResult> RollbackPromotionAsync(int fromClassId, int toClassId, string academicYear)
    {
        var result = new BulkPromotionResult
        {
            PromotionDate = DateTime.UtcNow,
            AcademicYear = academicYear
        };

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Identify ONLY the students this promotion actually moved, from the promotion
            // history — NOT the entire target class, which may contain students who were
            // already enrolled there before the promotion. Moving the whole class back would
            // corrupt the records of those pre-existing students.
            var historyRows = await _context.StudentPromotionHistory
                .Where(h => h.FromClassId == fromClassId && h.ToClassId == toClassId)
                .ToListAsync();

            var promotedStudentIds = historyRows.Select(h => h.StudentId).Distinct().ToList();
            result.TotalStudents = promotedStudentIds.Count;

            if (promotedStudentIds.Count > 0)
            {
                // Move back only the promoted students that are still in the target class.
                var studentsToRevert = (await _studentRepository.GetStudentsByClassIdAsync(toClassId))
                    .Where(s => promotedStudentIds.Contains(s.Id))
                    .ToList();

                foreach (var student in studentsToRevert)
                {
                    student.ClassId = fromClassId;
                    student.UpdatedAt = DateTime.UtcNow;
                }
                // Update students AND remove the promotion-history rows in a single
                // SaveChanges inside one transaction, so the rollback is fully atomic.
                // (Do NOT call UpdateMultipleStudentsAsync here — it opens its own
                // transaction, and SQLite does not support nested transactions.)
                _context.Students.UpdateRange(studentsToRevert);
                _context.StudentPromotionHistory.RemoveRange(historyRows);
                await _context.SaveChangesAsync();

                result.PromotedStudents = studentsToRevert.Count;
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Errors.Add(new PromotionError
            {
                StudentId = 0,
                StudentName = "System",
                Error = $"Rollback failed: {ex.Message}"
            });
        }

        return result;
    }


}