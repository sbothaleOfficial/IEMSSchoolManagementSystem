using IEMS.Application.DTOs;
using IEMS.Core.Interfaces;
using IEMS.Core.Services;
using IEMS.Core.Entities;
using IEMS.Core.Configuration;

namespace IEMS.Application.Services;

public class BulkPromotionService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IClassRepository _classRepository;
    private readonly IFeePaymentRepository _feePaymentRepository;
    private readonly StudentPromotionService _promotionService;
    private readonly StudentEligibilityValidator _eligibilityValidator;
    private readonly ClassProgressionValidator _progressionValidator;
    private readonly IStudentPromotionRepository _promotionRepository;

    public BulkPromotionService(
        IStudentRepository studentRepository,
        IClassRepository classRepository,
        IFeePaymentRepository feePaymentRepository,
        StudentPromotionService promotionService,
        StudentEligibilityValidator eligibilityValidator,
        ClassProgressionValidator progressionValidator,
        IStudentPromotionRepository promotionRepository)
    {
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _feePaymentRepository = feePaymentRepository;
        _promotionService = promotionService;
        _eligibilityValidator = eligibilityValidator;
        _progressionValidator = progressionValidator;
        _promotionRepository = promotionRepository;
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

                    // Build the history rows and re-point each student to the target class.
                    var historyRows = new List<StudentPromotionHistory>();
                    foreach (var student in studentsToPromote)
                    {
                        historyRows.Add(new StudentPromotionHistory
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
                        });

                        student.ClassId = request.ToClassId;
                        student.UpdatedAt = DateTime.UtcNow;
                    }

                    // Update the students AND insert the history atomically (one transaction).
                    await _promotionRepository.PromoteAsync(studentsToPromote, historyRows);
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

        try
        {
            // Identify ONLY the students this promotion actually moved, from the promotion
            // history — NOT the entire target class, which may contain students who were
            // already enrolled there before the promotion. Moving the whole class back would
            // corrupt the records of those pre-existing students.
            var historyRows = await _promotionRepository.GetHistoryAsync(fromClassId, toClassId);

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

                // Revert the students AND remove the history rows atomically (one transaction).
                await _promotionRepository.RollbackAsync(studentsToRevert, historyRows);

                result.PromotedStudents = studentsToRevert.Count;
            }
        }
        catch (Exception ex)
        {
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