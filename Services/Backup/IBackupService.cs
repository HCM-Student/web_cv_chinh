using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WEB_CV.Services.Backup
{
    public enum RestoreMode { Auto, FilesOnly, DatabaseOnly }

    public class BackupRecord
    {
        public string FileName { get; set; } = "";
        public long SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = ""; // full | files | db
    }

    public class BackupSettings
    {
        public bool Enabled { get; set; } = false;
        public string Frequency { get; set; } = "Weekly"; // Daily | Weekly | Monthly
        public string TimeOfDay { get; set; } = "02:00";  // 24h HH:mm
        public int RetentionCount { get; set; } = 10;
        public string StoragePath { get; set; } = "backups";
        public List<string> IncludeFolders { get; set; } = new List<string> { "wwwroot" };
    }

    public interface IBackupService
    {
        string GetBackupRoot();
        BackupSettings GetSettings();
        void SaveSettings(BackupSettings settings);
        IEnumerable<BackupRecord> GetBackupHistory();

        Task<string> BackupFilesAsync(CancellationToken ct = default);
        Task<string> BackupDatabaseAsync(CancellationToken ct = default);
        Task<string> BackupFullAsync(CancellationToken ct = default);

        Task RestoreAsync(string fileName, RestoreMode mode = RestoreMode.Auto, CancellationToken ct = default);
        bool DeleteBackup(string fileName);
        string? GetBackupFilePath(string fileName);
        void EnforceRetention();
    }
}
