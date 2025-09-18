using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient; // <— dùng Microsoft.Data.SqlClient

namespace WEB_CV.Services.Backup
{
    public class BackupService : IBackupService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly ILogger<BackupService> _logger;

        private readonly string _backupRoot;
        private readonly string _settingsPath;
        private readonly string _connectionString;

        public BackupService(IWebHostEnvironment env, IConfiguration config, ILogger<BackupService> logger)
        {
            _env = env;
            _config = config;
            _logger = logger;

            _connectionString = _config.GetConnectionString("DefaultConnection") ?? "";

            var cfgPath = _config["Backup:StoragePath"] ?? "backups";
            _backupRoot = Path.IsPathRooted(cfgPath)
                ? cfgPath
                : Path.Combine(_env.ContentRootPath, cfgPath);
            Directory.CreateDirectory(_backupRoot);
            _settingsPath = Path.Combine(_backupRoot, "_settings.json");

            if (!File.Exists(_settingsPath))
            {
                SaveSettings(new BackupSettings());
            }
        }

        public string GetBackupRoot() => _backupRoot;

        public BackupSettings GetSettings()
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<BackupSettings>(json) ?? new BackupSettings();
            }
            catch
            {
                return new BackupSettings();
            }
        }

        public void SaveSettings(BackupSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }

        private string Timestamp() => DateTime.Now.ToString("yyyy_MM_dd_HH_mm");

        public IEnumerable<BackupRecord> GetBackupHistory()
        {
            var allFiles = new List<System.IO.FileInfo>();

            try
            {
                if (System.IO.Directory.Exists(_backupRoot))
                    allFiles.AddRange(System.IO.Directory.GetFiles(_backupRoot)
                        .Select(f => new System.IO.FileInfo(f)));
            }
            catch {}

            try
            {
                var serverDir = GetSqlServerDefaultBackupDir();
                if (!string.IsNullOrWhiteSpace(serverDir) && System.IO.Directory.Exists(serverDir))
                    allFiles.AddRange(System.IO.Directory.GetFiles(serverDir, "*.bak")
                        .Select(f => new System.IO.FileInfo(f)));
            }
            catch {}

            foreach (var fi in allFiles.OrderByDescending(f => f.CreationTimeUtc))
            {
                var type = fi.Extension.ToLowerInvariant() switch
                {
                    ".zip" => fi.Name.StartsWith("full_backup") ? "full" : "files",
                    ".bak" => "db",
                    ".db"  => "db",
                    ".sql" => "db",
                    _      => "unknown"
                };
                yield return new BackupRecord
                {
                    FileName = fi.Name,
                    SizeBytes = fi.Length,
                    CreatedAt = fi.CreationTime,
                    Type = type
                };
            }
        }

        public async Task<string> BackupFilesAsync(CancellationToken ct = default)
        {
            var settings = GetSettings();
            var stamp = Timestamp();
            var zipPath = Path.Combine(_backupRoot, $"files_backup_{stamp}.zip");

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var relative in settings.IncludeFolders.Distinct())
                {
                    var folder = Path.IsPathRooted(relative)
                        ? relative
                        : Path.Combine(_env.ContentRootPath, relative);

                    if (!Directory.Exists(folder)) continue;

                    var baseLen = folder.TrimEnd(Path.DirectorySeparatorChar).Length + 1;
                    foreach (var filePath in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                    {
                        ct.ThrowIfCancellationRequested();
                        var entryName = Path.Combine(Path.GetFileName(folder), filePath.Substring(baseLen));
                        zip.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
                    }
                }
            }

            _logger.LogInformation("Files backup created: {zip}", zipPath);
            EnforceRetention();
            return zipPath;
        }

        public async Task<string> BackupDatabaseAsync(CancellationToken ct = default)
        {
            var lower = (_connectionString ?? "").ToLowerInvariant();
            var stamp = Timestamp();

            // ==== SQLITE ====
            if (lower.Contains(".db") || lower.Contains("sqlite"))
            {
                // Lấy đường dẫn Data Source=...
                var m = Regex.Match(_connectionString, @"Data Source=(?<path>[^;]+)", RegexOptions.IgnoreCase);
                var dbPath = m.Success ? m.Groups["path"].Value : null;
                if (string.IsNullOrWhiteSpace(dbPath))
                    throw new InvalidOperationException("Không tìm thấy 'Data Source=' trong ConnectionStrings:DefaultConnection cho SQLite.");

                // Nếu là relative => ghép với ContentRoot
                if (!Path.IsPathRooted(dbPath))
                    dbPath = Path.GetFullPath(Path.Combine(_env.ContentRootPath, dbPath));

                if (!File.Exists(dbPath))
                    throw new FileNotFoundException($"Không thấy file SQLite: {dbPath}");

                var dest = Path.Combine(_backupRoot, $"db_backup_{stamp}.db");
                File.Copy(dbPath, dest, overwrite: true);
                _logger.LogInformation("SQLite DB copied from {src} to {dest}", dbPath, dest);
                EnforceRetention();
                return dest;
            }

            // ==== SQL SERVER ====
            // Lấy database name
            string dbName = "";
            var mDb = Regex.Match(_connectionString, @"(Initial Catalog|Database)=(?<db>[^;]+)", RegexOptions.IgnoreCase);
            if (mDb.Success) dbName = mDb.Groups["db"].Value;
            if (string.IsNullOrWhiteSpace(dbName))
                throw new InvalidOperationException("Không tìm thấy Database/Initial Catalog trong chuỗi kết nối SQL Server.");

            // 1) Thử backup TRỰC TIẾP vào thư mục cấu hình (backupRoot)
            var fileName = $"db_backup_{stamp}.bak";
            var destPreferred = Path.Combine(_backupRoot, fileName);
            var backupSql = $@"
BACKUP DATABASE [{dbName}]
TO DISK = N'{destPreferred.Replace("'", "''")}'
WITH COPY_ONLY, INIT, NAME = N'{dbName}-Full-{stamp}', SKIP, STATS = 5;";

            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
                using var cmd  = new Microsoft.Data.SqlClient.SqlCommand(backupSql, conn) { CommandTimeout = 60 * 30 };
                await conn.OpenAsync(ct);
                await cmd.ExecuteNonQueryAsync(ct);

                _logger.LogInformation("SQL Server DB backup created at {dest}", destPreferred);
                EnforceRetention();
                return destPreferred;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 3201 || ex.Number == 15105)
            {
                // 3201/15105: SQL Server KHÔNG ghi được vào thư mục web app (thiếu quyền/khác máy)
                _logger.LogWarning(ex, "Không ghi được vào {destPreferred}. Sẽ fallback qua thư mục backup mặc định của SQL Server.", destPreferred);

                // 2) Fallback: hỏi đường dẫn backup mặc định của SQL Server, backup vào đó
                string? serverDefaultBackupDir = null;
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT CONVERT(varchar(260), SERVERPROPERTY('InstanceDefaultBackupPath'))", conn))
                {
                    await conn.OpenAsync(ct);
                    var val = await cmd.ExecuteScalarAsync(ct);
                    serverDefaultBackupDir = (val as string)?.Trim();
                }
                if (string.IsNullOrWhiteSpace(serverDefaultBackupDir))
                    throw new InvalidOperationException("SQL Server không cung cấp 'InstanceDefaultBackupPath'. Hãy đặt Backup:StoragePath tới thư mục mà dịch vụ SQL Server có quyền ghi.");

                var serverSidePath = Path.Combine(serverDefaultBackupDir, fileName).Replace('\\','/');

                var backupSqlDefault = $@"
BACKUP DATABASE [{dbName}]
TO DISK = N'{serverSidePath.Replace("'", "''")}'
WITH COPY_ONLY, INIT, NAME = N'{dbName}-Full-{stamp}', SKIP, STATS = 5;";

                using (var conn2 = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
                using (var cmd2  = new Microsoft.Data.SqlClient.SqlCommand(backupSqlDefault, conn2) { CommandTimeout = 60 * 30 })
                {
                    await conn2.OpenAsync(ct);
                    await cmd2.ExecuteNonQueryAsync(ct);
                }

                // Nếu SQL Server chạy cùng máy web: cố gắng copy file về thư mục backup của web
                try
                {
                    if (File.Exists(serverSidePath))
                    {
                        var final = Path.Combine(_backupRoot, fileName);
                        File.Copy(serverSidePath, final, overwrite: true);
                        _logger.LogInformation("Đã backup vào {serverSidePath} và copy về {final}", serverSidePath, final);
                        EnforceRetention();
                        return final;
                    }
                }
                catch (Exception copyEx)
                {
                    _logger.LogWarning(copyEx, "Đã backup vào {serverSidePath} nhưng không copy được về web folder. Hãy tải file thủ công.", serverSidePath);
                    // Trả về đường dẫn phía SQL Server (có thể khác máy)
                    return serverSidePath;
                }

                // Nếu tới đây: không copy được về web folder
                return serverSidePath;
            }
        }

        public async Task<string> BackupFullAsync(CancellationToken ct = default)
        {
            var db = await BackupDatabaseAsync(ct);
            var files = await BackupFilesAsync(ct);

            var stamp = Timestamp();
            var finalZip = Path.Combine(_backupRoot, $"full_backup_{stamp}.zip");
            using (var zip = ZipFile.Open(finalZip, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(db, Path.GetFileName(db), CompressionLevel.Optimal);
                zip.CreateEntryFromFile(files, Path.GetFileName(files), CompressionLevel.Optimal);
            }

            _logger.LogInformation("Full backup created: {finalZip}", finalZip);
            EnforceRetention();
            return finalZip;
        }

        public async Task RestoreAsync(string fileName, RestoreMode mode = RestoreMode.Auto, CancellationToken ct = default)
        {
            var path = GetBackupFilePath(fileName) ?? throw new FileNotFoundException("Backup không tồn tại.");
            var ext = Path.GetExtension(path).ToLowerInvariant();

            var isSqlite = (_connectionString ?? "").ToLowerInvariant().Contains(".db") ||
                           (_connectionString ?? "").ToLowerInvariant().Contains("sqlite");

            if (ext == ".zip")
            {
                using var zip = ZipFile.OpenRead(path);
                foreach (var entry in zip.Entries)
                {
                    ct.ThrowIfCancellationRequested();
                    if (entry.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) &&
                        (mode == RestoreMode.Auto || mode == RestoreMode.FilesOnly))
                    {
                        var tmpFilesZip = Path.Combine(_backupRoot, "_tmp_files.zip");
                        entry.ExtractToFile(tmpFilesZip, overwrite: true);
                        await RestoreFilesZip(tmpFilesZip, ct);
                        File.Delete(tmpFilesZip);
                    }
                    if ((entry.Name.EndsWith(".bak", StringComparison.OrdinalIgnoreCase) ||
                         entry.Name.EndsWith(".db",  StringComparison.OrdinalIgnoreCase) ||
                         entry.Name.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)) &&
                        (mode == RestoreMode.Auto || mode == RestoreMode.DatabaseOnly))
                    {
                        var tmpDb = Path.Combine(_backupRoot, "_tmp_db" + Path.GetExtension(entry.Name));
                        entry.ExtractToFile(tmpDb, overwrite: true);
                        await RestoreDatabase(tmpDb, isSqlite, ct);
                        File.Delete(tmpDb);
                    }
                }
                return;
            }

            if (ext == ".bak" || ext == ".db" || ext == ".sql")
            {
                await RestoreDatabase(path, isSqlite, ct);
                return;
            }

            // nếu là zip files riêng lẻ (đã handle phía trên)
        }

        private async Task RestoreFilesZip(string zipPath, CancellationToken ct)
        {
            using var zip = ZipFile.OpenRead(zipPath);
            foreach (var entry in zip.Entries)
            {
                ct.ThrowIfCancellationRequested();

                // chống path traversal
                var safeName = entry.FullName.Replace('\\', '/');
                if (safeName.Contains("..")) continue;

                var dest = Path.Combine(_env.ContentRootPath, safeName);
                var destDir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(destDir)) Directory.CreateDirectory(destDir);
                entry.ExtractToFile(dest, overwrite: true);
            }
            _logger.LogWarning("Files restored from {zip}", zipPath);
        }

        private async Task RestoreDatabase(string backupPath, bool isSqlite, CancellationToken ct)
        {
            if (isSqlite)
            {
                var m = Regex.Match(_connectionString, @"Data Source=(?<path>[^;]+)", RegexOptions.IgnoreCase);
                var dbPath = m.Success ? m.Groups["path"].Value : null;
                if (string.IsNullOrWhiteSpace(dbPath))
                    throw new InvalidOperationException("Không thể tìm Data Source trong ConnectionString cho SQLite.");

                File.Copy(backupPath, dbPath, overwrite: true);
                _logger.LogWarning("SQLite database restored from {src} to {dest}. Có thể cần khởi động lại ứng dụng.", backupPath, dbPath);
                return;
            }
            else
            {
                string dbName = "";
                var mDb = Regex.Match(_connectionString, @"(Initial Catalog|Database)=(?<db>[^;]+)", RegexOptions.IgnoreCase);
                if (mDb.Success) dbName = mDb.Groups["db"].Value;
                if (string.IsNullOrWhiteSpace(dbName)) throw new InvalidOperationException("Không tìm thấy Database/Initial Catalog.");

                var setSingleUser = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                var restore = $@"RESTORE DATABASE [{dbName}] FROM DISK = N'{backupPath.Replace("'", "''")}' WITH REPLACE, RECOVERY;";
                var setMultiUser = $"ALTER DATABASE [{dbName}] SET MULTI_USER;";

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(setSingleUser + restore + setMultiUser, conn) { CommandTimeout = 60 * 30 })
                {
                    await conn.OpenAsync(ct);
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                _logger.LogWarning("SQL Server database [{db}] restored from {path}", dbName, backupPath);
            }
        }

        public bool DeleteBackup(string fileName)
        {
            var path = GetBackupFilePath(fileName);
            if (path == null) return false;
            File.Delete(path);
            return true;
        }

        private string? GetSqlServerDefaultBackupDir()
        {
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
                using var cmd  = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT CONVERT(varchar(260), SERVERPROPERTY('InstanceDefaultBackupPath'))", conn);
                conn.Open();
                var val = cmd.ExecuteScalar() as string;
                if (!string.IsNullOrWhiteSpace(val))
                {
                    var path = val.Trim();
                    if (!System.IO.Path.IsPathRooted(path)) return null;
                    if (System.IO.Directory.Exists(path)) return path;
                }
            }
            catch { }
            return null;
        }

        public string? GetBackupFilePath(string fileName)
        {
            var local = System.IO.Path.Combine(_backupRoot, fileName);
            if (System.IO.File.Exists(local)) return local;

            var serverDir = GetSqlServerDefaultBackupDir();
            if (!string.IsNullOrWhiteSpace(serverDir))
            {
                var serverPath = System.IO.Path.Combine(serverDir, fileName);
                if (System.IO.File.Exists(serverPath)) return serverPath;
            }
            return null;
        }

        public void EnforceRetention()
        {
            var settings = GetSettings();
            var files = Directory.GetFiles(_backupRoot)
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTimeUtc)
                .ToList();

            if (files.Count <= settings.RetentionCount) return;

            foreach (var fi in files.Skip(settings.RetentionCount))
            {
                try { fi.Delete(); } catch { /* ignore */ }
            }
        }
    }
}
