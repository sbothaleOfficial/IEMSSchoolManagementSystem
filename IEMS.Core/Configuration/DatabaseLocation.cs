using System;
using System.IO;

namespace IEMS.Core.Configuration
{
    /// <summary>
    /// Single source of truth for where the SQLite database lives.
    ///
    /// Uses an absolute path next to the application executable (AppContext.BaseDirectory)
    /// instead of a working-directory-relative "school.db". With the old relative path,
    /// launching the app from a different folder silently opened/created a DIFFERENT school.db —
    /// which looked to users like "all my data disappeared". In the normal double-click case the
    /// location is unchanged (the working directory already equals the exe folder), so this is a
    /// safe, defensive choice.
    /// </summary>
    public static class DatabaseLocation
    {
        public static string DatabaseFilePath { get; } =
            Path.Combine(AppContext.BaseDirectory, "school.db");

        public static string ConnectionString { get; } =
            $"Data Source={DatabaseFilePath}";
    }
}
