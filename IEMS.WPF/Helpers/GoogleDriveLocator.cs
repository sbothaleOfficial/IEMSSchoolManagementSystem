using System;
using System.Collections.Generic;
using System.IO;

namespace IEMS.WPF.Helpers
{
    /// <summary>
    /// Best-effort detection of a "Google Drive for Desktop" location on this PC. Google Drive for
    /// Desktop exposes the user's Drive either as a virtual drive letter (streaming mode, e.g.
    /// G:\My Drive) or as a folder under the profile (mirror / older Backup &amp; Sync). We just need a
    /// real folder that Google syncs, so backups dropped there are uploaded to Drive automatically —
    /// no API or account credentials required.
    /// </summary>
    public static class GoogleDriveLocator
    {
        /// <summary>The user's "My Drive" folder if Google Drive for Desktop is present, else null.</summary>
        public static string? FindMyDriveFolder()
        {
            foreach (var candidate in CandidateRoots())
            {
                try
                {
                    if (Directory.Exists(candidate)) return candidate;
                }
                catch { /* inaccessible drive — skip */ }
            }
            return null;
        }

        public static bool IsAvailable() => FindMyDriveFolder() != null;

        private static IEnumerable<string> CandidateRoots()
        {
            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            // Mirror mode / older Backup & Sync place a folder under the user profile.
            yield return Path.Combine(profile, "My Drive");
            yield return Path.Combine(profile, "Google Drive");

            // Streaming mode mounts a virtual drive (often G:, but the letter is user-configurable),
            // whose root contains "My Drive". Scan every drive for it.
            DriveInfo[] drives;
            try { drives = DriveInfo.GetDrives(); }
            catch { drives = Array.Empty<DriveInfo>(); }

            foreach (var d in drives)
            {
                string root;
                try { root = d.RootDirectory.FullName; }
                catch { continue; }
                yield return Path.Combine(root, "My Drive");
            }
        }
    }
}
