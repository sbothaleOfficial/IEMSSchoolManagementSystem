using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace IEMS.Application.Services
{
    public class BackupService : IBackupService
    {
        private readonly string _databasePath;
        private readonly string _backupRootPath;
        private readonly string _metadataPath;
        private readonly string _connectionString;
        private readonly IServiceProvider? _serviceProvider;
        private static readonly object _metadataLock = new object();

        public BackupService(IServiceProvider? serviceProvider = null)
        {
            _serviceProvider = serviceProvider;
            // Use the same single source of truth as EF Core (absolute path next to the exe),
            // so backup/restore always targets the database the app actually opens.
            _databasePath = IEMS.Infrastructure.Data.DatabaseLocation.DatabaseFilePath;
            _connectionString = IEMS.Infrastructure.Data.DatabaseLocation.ConnectionString;
            _backupRootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "IEMS_Backups");
            _metadataPath = Path.Combine(_backupRootPath, "backup_metadata.json");

            // Ensure backup directory exists
            Directory.CreateDirectory(_backupRootPath);
        }

        public async Task<BackupResult> CreateBackupAsync(BackupType backupType, string? customPath = null)
        {
            try
            {
                if (!File.Exists(_databasePath))
                    return new BackupResult { Success = false, Message = "Database file not found" };

                // Properly close database connections and checkpoint WAL
                await CloseDatabaseConnectionsAsync();

                var backupInfo = new BackupInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = backupType,
                    Timestamp = DateTime.Now,
                    SourcePath = _databasePath
                };

                // Generate backup filename with timestamp
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupFileName = $"school_backup_{timestamp}_{backupType}.db";

                // Determine backup path
                string backupPath;
                if (!string.IsNullOrEmpty(customPath))
                {
                    backupPath = Path.Combine(customPath, backupFileName);
                }
                else if (backupType == BackupType.Manual)
                {
                    // For manual backups, save to Desktop
                    var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    backupPath = Path.Combine(desktopPath, "IEMS_Backups", backupFileName);

                    // FIXED BUG #10: Check for null before creating directory
                    var backupDir = Path.GetDirectoryName(backupPath);
                    if (!string.IsNullOrEmpty(backupDir))
                        Directory.CreateDirectory(backupDir);
                }
                else
                {
                    // Automatic backups go to Documents folder
                    backupPath = Path.Combine(_backupRootPath, GetBackupSubfolder(backupType), backupFileName);

                    // FIXED BUG #10: Check for null before creating directory
                    var backupDir = Path.GetDirectoryName(backupPath);
                    if (!string.IsNullOrEmpty(backupDir))
                        Directory.CreateDirectory(backupDir);
                }

                // Handle backup type logic
                // NOTE: For SQLite databases, "incremental" backups are actually full backups
                // because SQLite is a single-file database and does not support true incremental backups
                // like file-based systems. The incremental logic ensures we have periodic full backups.
                if (backupType == BackupType.Incremental)
                {
                    var lastFullBackup = await GetLastFullBackupAsync();
                    if (lastFullBackup == null || (DateTime.Now - lastFullBackup.Timestamp).TotalDays > 7)
                    {
                        // Force full backup if no recent full backup exists
                        backupType = BackupType.Full;
                        backupInfo.Type = BackupType.Full;
                    }
                    else
                    {
                        // For SQLite, incremental is a full copy but with different retention policy
                        // This allows more frequent "incremental" backups with shorter retention
                        backupInfo.Type = BackupType.Incremental;
                    }
                }

                // Copy the database file
                File.Copy(_databasePath, backupPath, true);

                // FIXED BUG #2: Also copy WAL and SHM files if they exist (for consistency)
                // Any failure in copying these files should fail the entire backup
                var walPath = _databasePath + "-wal";
                var shmPath = _databasePath + "-shm";

                try
                {
                    if (File.Exists(walPath))
                        File.Copy(walPath, backupPath + "-wal", true);
                    if (File.Exists(shmPath))
                        File.Copy(shmPath, backupPath + "-shm", true);
                }
                catch (Exception ex)
                {
                    // If WAL/SHM copy fails, cleanup and fail the backup
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                    if (File.Exists(backupPath + "-wal"))
                        File.Delete(backupPath + "-wal");
                    if (File.Exists(backupPath + "-shm"))
                        File.Delete(backupPath + "-shm");

                    throw new InvalidOperationException($"Failed to copy WAL/SHM files: {ex.Message}", ex);
                }

                // Calculate checksum for integrity verification
                backupInfo.Checksum = await CalculateChecksumAsync(backupPath);
                backupInfo.BackupPath = backupPath;
                backupInfo.FileSize = new FileInfo(backupPath).Length;

                // Verify backup integrity
                var verificationResult = await VerifyBackupIntegrityAsync(backupPath);
                if (!verificationResult.IsValid)
                {
                    // Delete the corrupted backup file
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);

                    return new BackupResult
                    {
                        Success = false,
                        Message = $"Backup verification failed: {verificationResult.Message}. The corrupted backup file has been deleted."
                    };
                }

                // Save backup metadata
                await SaveBackupMetadataAsync(backupInfo);

                // Clean old backups based on retention policy
                await CleanOldBackupsAsync(backupType);

                return new BackupResult
                {
                    Success = true,
                    Message = $"Backup created successfully at {backupPath}",
                    BackupPath = backupPath,
                    BackupInfo = backupInfo
                };
            }
            catch (Exception ex)
            {
                return new BackupResult
                {
                    Success = false,
                    Message = $"Backup failed: {ex.Message}",
                    Error = ex
                };
            }
        }

        public async Task<RestoreResult> RestoreBackupAsync(string backupPath, bool validateChecksum = true, bool skipSafetyBackup = false)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return new RestoreResult { Success = false, Message = "Backup file not found" };

                // Validate backup file
                var validationResult = await ValidateBackupFileAsync(backupPath);
                if (!validationResult.IsValid)
                    return new RestoreResult { Success = false, Message = validationResult.Message };

                // Validate checksum if requested
                if (validateChecksum)
                {
                    var checksumResult = await ValidateBackupChecksumAsync(backupPath);
                    if (!checksumResult.IsValid)
                        return new RestoreResult { Success = false, Message = checksumResult.Message };
                }

                // Create a safety backup of current database before restoring (unless skipped)
                BackupResult? safetyBackup = null;
                if (!skipSafetyBackup)
                {
                    safetyBackup = await CreateBackupAsync(BackupType.PreRestore);
                    if (!safetyBackup.Success)
                    {
                        return new RestoreResult
                        {
                            Success = false,
                            Message = "Failed to create safety backup before restore. Current database may be corrupted.",
                            RequiresConfirmation = true,
                            ConfirmationMessage = "Cannot create safety backup of current database. This may indicate the current database is corrupted.\n\n" +
                                                "Do you want to proceed with restore anyway? (This will overwrite the current database)"
                        };
                    }
                }

                // Check if current database has newer data
                var currentDbInfo = new FileInfo(_databasePath);
                var backupDbInfo = new FileInfo(backupPath);

                if (currentDbInfo.LastWriteTime > backupDbInfo.LastWriteTime)
                {
                    // FIXED BUG #12: Better time difference display showing hours for recent backups
                    var timeDiff = currentDbInfo.LastWriteTime - backupDbInfo.LastWriteTime;
                    string timeDiffStr;
                    if (timeDiff.TotalHours < 24)
                    {
                        timeDiffStr = $"{(int)timeDiff.TotalHours} hours";
                    }
                    else if (timeDiff.TotalDays < 7)
                    {
                        timeDiffStr = $"{timeDiff.Days} days";
                    }
                    else
                    {
                        timeDiffStr = $"{timeDiff.Days} days ({timeDiff.Days / 7} weeks)";
                    }

                    return new RestoreResult
                    {
                        Success = false,
                        RequiresConfirmation = true,
                        Message = $"Current database is {timeDiffStr} newer than the backup. Are you sure you want to restore?",
                        SafetyBackupPath = safetyBackup?.BackupPath
                    };
                }

                // Properly close database connections and checkpoint WAL
                await CloseDatabaseConnectionsAsync();

                // Additional aggressive cleanup for restore operation
                await ForceCloseAllDatabaseFilesAsync();

                // Perform the restore with retry logic
                await RestoreDatabaseFileWithRetryAsync(backupPath);

                // FIXED BUG #3: Also restore WAL and SHM files if they exist
                // Any failure in restoring these files should fail the entire restore
                var walBackupPath = backupPath + "-wal";
                var shmBackupPath = backupPath + "-shm";

                try
                {
                    if (File.Exists(walBackupPath))
                        File.Copy(walBackupPath, _databasePath + "-wal", true);
                    if (File.Exists(shmBackupPath))
                        File.Copy(shmBackupPath, _databasePath + "-shm", true);
                }
                catch (Exception ex)
                {
                    // If WAL/SHM restore fails, the database is in an inconsistent state
                    throw new InvalidOperationException($"Failed to restore WAL/SHM files. Database may be in inconsistent state: {ex.Message}", ex);
                }

                // Log the restore operation
                await LogRestoreOperationAsync(backupPath, safetyBackup?.BackupPath);

                return new RestoreResult
                {
                    Success = true,
                    Message = "Database restored successfully. Please restart the application.",
                    RestoredFromPath = backupPath,
                    SafetyBackupPath = safetyBackup?.BackupPath,
                    RequiresRestart = true
                };
            }
            catch (Exception ex)
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = $"Restore failed: {ex.Message}",
                    Error = ex
                };
            }
        }

        public async Task<List<BackupInfo>> GetBackupHistoryAsync()
        {
            try
            {
                if (!File.Exists(_metadataPath))
                    return new List<BackupInfo>();

                var json = await File.ReadAllTextAsync(_metadataPath);
                var metadata = JsonSerializer.Deserialize<BackupMetadata>(json);

                // Sort by timestamp descending
                return metadata.Backups.OrderByDescending(b => b.Timestamp).ToList();
            }
            catch
            {
                return new List<BackupInfo>();
            }
        }

        public async Task<BackupScheduleResult> SetupAutomaticBackupAsync(BackupSchedule schedule)
        {
            try
            {
                // Save schedule configuration
                var schedulePath = Path.Combine(_backupRootPath, "backup_schedule.json");
                var json = JsonSerializer.Serialize(schedule, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(schedulePath, json);

                return new BackupScheduleResult
                {
                    Success = true,
                    Message = "Automatic backup schedule configured successfully"
                };
            }
            catch (Exception ex)
            {
                return new BackupScheduleResult
                {
                    Success = false,
                    Message = $"Failed to setup automatic backup: {ex.Message}"
                };
            }
        }

        private async Task CloseDatabaseConnectionsAsync()
        {
            try
            {
                // Close EF Core DbContext connections if available
                if (_serviceProvider != null)
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var dbContextTypes = new[] { "ApplicationDbContext", "IEMSDbContext" };

                        foreach (var contextTypeName in dbContextTypes)
                        {
                            var contextType = AppDomain.CurrentDomain.GetAssemblies()
                                .SelectMany(a => a.GetTypes())
                                .FirstOrDefault(t => t.Name == contextTypeName);

                            if (contextType != null)
                            {
                                var context = scope.ServiceProvider.GetService(contextType);
                                if (context is IDisposable disposable)
                                {
                                    disposable.Dispose();
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Continue with manual connection management if DI fails
                    }
                }

                // Force WAL checkpoint and close all connections
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "PRAGMA wal_checkpoint(TRUNCATE);";
                await command.ExecuteNonQueryAsync();

                command.CommandText = "PRAGMA optimize;";
                await command.ExecuteNonQueryAsync();

                await connection.CloseAsync();

                // Additional safety measures
                GC.Collect();
                GC.WaitForPendingFinalizers();
                await Task.Delay(500); // Allow time for cleanup
            }
            catch (Exception ex)
            {
                // Log but don't fail - backup should continue even if connection cleanup partially fails
                System.Diagnostics.Debug.WriteLine($"Database connection cleanup warning: {ex.Message}");
            }
        }

        private async Task ForceCloseAllDatabaseFilesAsync()
        {
            try
            {
                // Force close all SQLite connections by clearing the pool
                SqliteConnection.ClearAllPools();

                // Additional wait for file handles to be released
                await Task.Delay(1000);

                // Try to delete WAL and SHM files to force a clean state
                var walPath = _databasePath + "-wal";
                var shmPath = _databasePath + "-shm";

                try
                {
                    if (File.Exists(walPath))
                        File.Delete(walPath);
                }
                catch { /* Ignore if can't delete */ }

                try
                {
                    if (File.Exists(shmPath))
                        File.Delete(shmPath);
                }
                catch { /* Ignore if can't delete */ }

                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Force close database files warning: {ex.Message}");
            }
        }

        private async Task RestoreDatabaseFileWithRetryAsync(string backupPath)
        {
            const int maxRetries = 5;
            const int retryDelayMs = 1000;

            // Create temporary backup of current database for rollback
            string? tempBackupPath = null;
            string? tempWalPath = null;
            string? tempShmPath = null;

            try
            {
                // Create temp backup
                tempBackupPath = _databasePath + ".restore_temp_" + DateTime.Now.Ticks;
                if (File.Exists(_databasePath))
                {
                    File.Copy(_databasePath, tempBackupPath, true);

                    // Also backup WAL and SHM files if they exist
                    var walPath = _databasePath + "-wal";
                    var shmPath = _databasePath + "-shm";

                    if (File.Exists(walPath))
                    {
                        tempWalPath = walPath + ".restore_temp_" + DateTime.Now.Ticks;
                        File.Copy(walPath, tempWalPath, true);
                    }

                    if (File.Exists(shmPath))
                    {
                        tempShmPath = shmPath + ".restore_temp_" + DateTime.Now.Ticks;
                        File.Copy(shmPath, tempShmPath, true);
                    }
                }

                // Attempt restore with retry logic
                for (int retry = 0; retry < maxRetries; retry++)
                {
                    try
                    {
                        // Try to copy the backup file over the current database
                        File.Copy(backupPath, _databasePath, true);

                        // Success - delete temp backup and return
                        CleanupTempFiles(tempBackupPath, tempWalPath, tempShmPath);
                        return;
                    }
                    catch (UnauthorizedAccessException) when (retry < maxRetries - 1)
                    {
                        // File is locked, wait and retry
                        await Task.Delay(retryDelayMs * (retry + 1));

                        // Try more aggressive cleanup
                        SqliteConnection.ClearAllPools();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                    catch (IOException ex) when (ex.Message.Contains("user-mapped section") && retry < maxRetries - 1)
                    {
                        // Specific handling for the shared memory file issue
                        await Task.Delay(retryDelayMs * (retry + 1));

                        // Force connection pool cleanup
                        SqliteConnection.ClearAllPools();
                        await Task.Delay(500);
                    }
                }

                // If we get here, all retries failed - restore from temp backup
                throw new InvalidOperationException(
                    "Unable to restore database file. The database may be in use by another process.");
            }
            catch (Exception)
            {
                // Rollback - restore from temp backup
                if (tempBackupPath != null && File.Exists(tempBackupPath))
                {
                    try
                    {
                        File.Copy(tempBackupPath, _databasePath, true);

                        if (tempWalPath != null && File.Exists(tempWalPath))
                            File.Copy(tempWalPath, _databasePath + "-wal", true);

                        if (tempShmPath != null && File.Exists(tempShmPath))
                            File.Copy(tempShmPath, _databasePath + "-shm", true);

                        System.Diagnostics.Debug.WriteLine("Restore failed - database rolled back to previous state");
                    }
                    catch (Exception rollbackEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Rollback failed: {rollbackEx.Message}");
                    }
                    finally
                    {
                        CleanupTempFiles(tempBackupPath, tempWalPath, tempShmPath);
                    }
                }

                throw; // Re-throw the original exception
            }
        }

        private void CleanupTempFiles(string? tempBackupPath, string? tempWalPath, string? tempShmPath)
        {
            try
            {
                if (tempBackupPath != null && File.Exists(tempBackupPath))
                    File.Delete(tempBackupPath);
                if (tempWalPath != null && File.Exists(tempWalPath))
                    File.Delete(tempWalPath);
                if (tempShmPath != null && File.Exists(tempShmPath))
                    File.Delete(tempShmPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to cleanup temp files: {ex.Message}");
            }
        }

        // FIXED BUG #13: Use SHA256 instead of MD5 for better security and collision resistance
        private async Task<string> CalculateChecksumAsync(string filePath)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = await Task.Run(() => sha256.ComputeHash(stream));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private async Task<ValidationResult> VerifyBackupIntegrityAsync(string backupPath)
        {
            try
            {
                // First check the SQLite header
                var headerValidation = await ValidateBackupFileAsync(backupPath);
                if (!headerValidation.IsValid)
                    return headerValidation;

                // Try to open and verify the database
                var connectionString = $"Data Source={backupPath};Mode=ReadOnly";
                using (var connection = new SqliteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Run integrity check
                    var command = connection.CreateCommand();
                    command.CommandText = "PRAGMA integrity_check;";
                    var result = await command.ExecuteScalarAsync();

                    if (result?.ToString() != "ok")
                    {
                        return new ValidationResult
                        {
                            IsValid = false,
                            Message = $"Database integrity check failed: {result}"
                        };
                    }

                    // Check if we can query basic structure
                    command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table';";
                    await command.ExecuteScalarAsync();

                    await connection.CloseAsync();
                }

                return new ValidationResult
                {
                    IsValid = true,
                    Message = "Backup file verified successfully"
                };
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"Backup verification failed: {ex.Message}"
                };
            }
        }

        private async Task<ValidationResult> ValidateBackupFileAsync(string backupPath)
        {
            try
            {
                // Check if file is a valid SQLite database
                var header = new byte[16];
                using (var fs = new FileStream(backupPath, FileMode.Open, FileAccess.Read))
                {
                    await fs.ReadAsync(header, 0, 16);
                }

                var headerString = Encoding.ASCII.GetString(header);
                if (!headerString.StartsWith("SQLite format 3"))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "Invalid backup file - not a valid SQLite database"
                    };
                }

                return new ValidationResult
                {
                    IsValid = true,
                    Message = "Backup file is valid"
                };
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"Validation failed: {ex.Message}"
                };
            }
        }

        private async Task<ValidationResult> ValidateBackupChecksumAsync(string backupPath)
        {
            try
            {
                // Calculate current checksum
                var calculatedChecksum = await CalculateChecksumAsync(backupPath);

                // Try to find backup info in metadata
                var backupInfo = await GetBackupInfoByPathAsync(backupPath);
                if (backupInfo == null)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "Backup metadata not found - cannot verify integrity"
                    };
                }

                if (string.IsNullOrEmpty(backupInfo.Checksum))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "Backup checksum not available - cannot verify integrity"
                    };
                }

                if (calculatedChecksum != backupInfo.Checksum)
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "Backup file integrity check failed - file may be corrupted or tampered with"
                    };
                }

                return new ValidationResult
                {
                    IsValid = true,
                    Message = "Backup file integrity verified successfully"
                };
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"Checksum validation failed: {ex.Message}"
                };
            }
        }

        private async Task<BackupInfo?> GetBackupInfoByPathAsync(string backupPath)
        {
            try
            {
                var history = await GetBackupHistoryAsync();
                return history.FirstOrDefault(b =>
                    string.Equals(b.BackupPath, backupPath, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Path.GetFileName(b.BackupPath), Path.GetFileName(backupPath), StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return null;
            }
        }

        private string GetBackupSubfolder(BackupType type)
        {
            return type switch
            {
                BackupType.Full => "Full",
                BackupType.Incremental => "Incremental",
                BackupType.Daily => "Daily",
                BackupType.Weekly => "Weekly",
                BackupType.Monthly => "Monthly",
                BackupType.Manual => "Manual",
                BackupType.PreRestore => "PreRestore",
                _ => "Other"
            };
        }

        private async Task SaveBackupMetadataAsync(BackupInfo backupInfo)
        {
            await Task.Run(() =>
            {
                lock (_metadataLock) // Prevent concurrent access
                {
                    // Use atomic write with temp file to prevent corruption
                    var tempPath = _metadataPath + ".tmp";

                    try
                    {
                        var metadata = new BackupMetadata();

                        if (File.Exists(_metadataPath))
                        {
                            var json = File.ReadAllText(_metadataPath);
                            metadata = JsonSerializer.Deserialize<BackupMetadata>(json) ?? new BackupMetadata();
                        }

                        metadata.Backups.Add(backupInfo);
                        metadata.LastBackupDate = backupInfo.Timestamp;

                        var updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });

                        // Atomic write: temp file then move
                        File.WriteAllText(tempPath, updatedJson);
                        File.Move(tempPath, _metadataPath, true);
                    }
                    catch (Exception ex)
                    {
                        // Cleanup temp file on failure
                        if (File.Exists(tempPath))
                        {
                            try { File.Delete(tempPath); } catch { }
                        }
                        System.Diagnostics.Debug.WriteLine($"Failed to save backup metadata: {ex.Message}");
                        throw; // Re-throw to preserve original exception
                    }
                }
            });
        }

        // FIXED BUG #9: Return nullable BackupInfo to match FirstOrDefault behavior
        private async Task<BackupInfo?> GetLastFullBackupAsync()
        {
            var history = await GetBackupHistoryAsync();
            // Include both Full backups and Incremental backups that were converted to Full
            // This prevents the infinite loop where incremental backups keep getting converted to full
            return history
                .Where(b => b.Type == BackupType.Full ||
                           (b.Type == BackupType.Incremental && IsEffectivelyFullBackup(b)))
                .OrderByDescending(b => b.Timestamp)
                .FirstOrDefault();
        }

        private bool IsEffectivelyFullBackup(BackupInfo backup)
        {
            // For SQLite, all backups are effectively full backups since it's a single file
            // We consider any incremental backup as a full backup for the purpose of scheduling
            return backup.Type == BackupType.Incremental;
        }

        private async Task CleanOldBackupsAsync(BackupType type)
        {
            var history = await GetBackupHistoryAsync();
            var typeBackups = history.Where(b => b.Type == type).OrderByDescending(b => b.Timestamp).ToList();

            // Retention policy - improved to be more conservative
            var retentionDays = type switch
            {
                BackupType.Daily => 14,    // Increased from 7 to 14 days
                BackupType.Weekly => 60,   // Increased from 30 to 60 days
                BackupType.Monthly => 730, // Increased from 365 to 2 years
                BackupType.Full => 180,    // Increased from 90 to 180 days
                BackupType.Incremental => 30, // Increased from 14 to 30 days
                BackupType.PreRestore => 7,   // Increased from 3 to 7 days
                _ => 60
            };

            var cutoffDate = DateTime.Now.AddDays(-retentionDays);
            var backupsToDelete = typeBackups.Where(b => b.Timestamp < cutoffDate).Skip(2); // Keep at least 2

            var deletedBackupIds = new List<string>();

            foreach (var backup in backupsToDelete)
            {
                try
                {
                    bool mainFileDeleted = false;
                    bool walFileDeleted = true;  // Assume success if file doesn't exist
                    bool shmFileDeleted = true;  // Assume success if file doesn't exist

                    if (File.Exists(backup.BackupPath))
                    {
                        File.Delete(backup.BackupPath);
                        mainFileDeleted = true;

                        // FIXED BUG #15: Also delete associated WAL and SHM files with proper error logging
                        var walPath = backup.BackupPath + "-wal";
                        var shmPath = backup.BackupPath + "-shm";

                        if (File.Exists(walPath))
                        {
                            try
                            {
                                File.Delete(walPath);
                            }
                            catch (Exception walEx)
                            {
                                walFileDeleted = false;
                                System.Diagnostics.Debug.WriteLine($"Failed to delete WAL file {walPath}: {walEx.Message}");
                            }
                        }

                        if (File.Exists(shmPath))
                        {
                            try
                            {
                                File.Delete(shmPath);
                            }
                            catch (Exception shmEx)
                            {
                                shmFileDeleted = false;
                                System.Diagnostics.Debug.WriteLine($"Failed to delete SHM file {shmPath}: {shmEx.Message}");
                            }
                        }
                    }

                    // Only track as deleted if all files were successfully deleted
                    if (mainFileDeleted && walFileDeleted && shmFileDeleted)
                    {
                        deletedBackupIds.Add(backup.Id);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Partial deletion for backup {backup.BackupPath} - not removing from metadata");
                    }
                }
                catch (Exception ex)
                {
                    // Log specific errors but continue cleanup
                    System.Diagnostics.Debug.WriteLine($"Failed to delete backup {backup.BackupPath}: {ex.Message}");
                }
            }

            // Update metadata to remove deleted backups
            if (deletedBackupIds.Any())
            {
                await RemoveBackupsFromMetadataAsync(deletedBackupIds);
            }
        }

        private async Task RemoveBackupsFromMetadataAsync(List<string> backupIds)
        {
            await Task.Run(() =>
            {
                lock (_metadataLock) // Prevent concurrent access
                {
                    // Use atomic write with temp file to prevent corruption
                    var tempPath = _metadataPath + ".tmp";

                    try
                    {
                        var metadata = new BackupMetadata();

                        if (File.Exists(_metadataPath))
                        {
                            var json = File.ReadAllText(_metadataPath);
                            metadata = JsonSerializer.Deserialize<BackupMetadata>(json) ?? new BackupMetadata();
                        }

                        // Remove deleted backups from metadata
                        metadata.Backups = metadata.Backups.Where(b => !backupIds.Contains(b.Id)).ToList();

                        // Update last backup date
                        if (metadata.Backups.Any())
                        {
                            metadata.LastBackupDate = metadata.Backups.Max(b => b.Timestamp);
                        }
                        else
                        {
                            metadata.LastBackupDate = DateTime.MinValue;
                        }

                        // Save updated metadata using atomic write
                        var updatedJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });

                        // Atomic write: temp file then move
                        File.WriteAllText(tempPath, updatedJson);
                        File.Move(tempPath, _metadataPath, true);
                    }
                    catch (Exception ex)
                    {
                        // Cleanup temp file on failure
                        if (File.Exists(tempPath))
                        {
                            try { File.Delete(tempPath); } catch { }
                        }
                        System.Diagnostics.Debug.WriteLine($"Failed to update backup metadata: {ex.Message}");
                        throw; // Re-throw to preserve original exception
                    }
                }
            });
        }

        private async Task LogRestoreOperationAsync(string restoredFrom, string? safetyBackupPath)
        {
            var logPath = Path.Combine(_backupRootPath, "restore_log.json");
            var log = new RestoreLog
            {
                Timestamp = DateTime.Now,
                RestoredFrom = restoredFrom,
                SafetyBackupPath = safetyBackupPath ?? string.Empty
            };

            var logs = new List<RestoreLog>();
            if (File.Exists(logPath))
            {
                var json = await File.ReadAllTextAsync(logPath);
                logs = JsonSerializer.Deserialize<List<RestoreLog>>(json) ?? new List<RestoreLog>();
            }

            logs.Add(log);
            var updatedJson = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(logPath, updatedJson);
        }

        public async Task RemoveBackupFromMetadataAsync(string backupId)
        {
            await RemoveBackupsFromMetadataAsync(new List<string> { backupId });
        }
    }

    public interface IBackupService
    {
        Task<BackupResult> CreateBackupAsync(BackupType backupType, string? customPath = null);
        Task<RestoreResult> RestoreBackupAsync(string backupPath, bool validateChecksum = true, bool skipSafetyBackup = false);
        Task<List<BackupInfo>> GetBackupHistoryAsync();
        Task<BackupScheduleResult> SetupAutomaticBackupAsync(BackupSchedule schedule);
        Task RemoveBackupFromMetadataAsync(string backupId);
    }

    public enum BackupType
    {
        Full,
        Incremental,
        Daily,
        Weekly,
        Monthly,
        Manual,
        PreRestore
    }

    public class BackupInfo
    {
        public string Id { get; set; } = string.Empty;
        public BackupType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public string BackupPath { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Checksum { get; set; } = string.Empty;
    }

    public class BackupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? BackupPath { get; set; }
        public BackupInfo? BackupInfo { get; set; }
        public Exception? Error { get; set; }
    }

    public class RestoreResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RestoredFromPath { get; set; }
        public string? SafetyBackupPath { get; set; }
        public bool RequiresConfirmation { get; set; }
        public string? ConfirmationMessage { get; set; }
        public bool RequiresRestart { get; set; }
        public Exception? Error { get; set; }
    }

    public class BackupSchedule
    {
        public bool EnableDaily { get; set; }
        public bool EnableWeekly { get; set; }
        public bool EnableMonthly { get; set; }
        public TimeSpan DailyTime { get; set; }
        public DayOfWeek WeeklyDay { get; set; }
        public int MonthlyDay { get; set; }
        public bool EnableIncremental { get; set; }
        public int IncrementalIntervalHours { get; set; }
    }

    public class BackupScheduleResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BackupMetadata
    {
        public List<BackupInfo> Backups { get; set; } = new List<BackupInfo>();
        public DateTime LastBackupDate { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class RestoreLog
    {
        public DateTime Timestamp { get; set; }
        public string RestoredFrom { get; set; } = string.Empty;
        public string SafetyBackupPath { get; set; } = string.Empty;
    }
}