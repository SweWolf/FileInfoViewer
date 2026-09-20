using FileInfoViewer.Models;
using Microsoft.Data.Sqlite;

namespace FileInfoViewer.Services;

internal static class SqliteInfoReader
{
    // SQLite files always start with this 16-byte magic string
    private static readonly byte[] Magic = "SQLite format 3\0"u8.ToArray();

    public static bool IsSqliteFile(string filePath)
    {
        try
        {
            Span<byte> buf = stackalloc byte[16];
            using var fs = File.OpenRead(filePath);
            return fs.Read(buf) == 16 && buf.SequenceEqual(Magic);
        }
        catch { return false; }
    }

    public static SqliteInfoModel Read(string filePath)
    {
        var model = new SqliteInfoModel();
        var cs = new SqliteConnectionStringBuilder
        {
            DataSource = filePath,
            Mode       = SqliteOpenMode.ReadOnly,
        }.ToString();

        using var conn = new SqliteConnection(cs);
        conn.Open();

        model.SqliteVersion = Scalar(conn, "SELECT sqlite_version()") ?? "";
        model.PageSizeBytes  = PragmaLong(conn, "page_size");
        model.PageCount      = PragmaLong(conn, "page_count");
        model.FreePageCount  = PragmaLong(conn, "freelist_count");
        model.TextEncoding   = PragmaStr(conn, "encoding") ?? "";
        model.JournalMode    = PragmaStr(conn, "journal_mode") ?? "";
        model.UserVersion    = (int)PragmaLong(conn, "user_version");
        model.ApplicationId  = (int)PragmaLong(conn, "application_id");

        // Schema objects
        model.TableCount = (int)CountSchema(conn, "table");
        model.ViewCount  = (int)CountSchema(conn, "view");
        model.IndexCount = (int)CountSchema(conn, "index");

        // Table names and row counts (cap at 100 tables)
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name LIMIT 100";
        cmd.CommandTimeout = 5;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.GetString(0);
            var entry = new SqliteTableInfo { Name = name };
            try
            {
                using var countCmd = conn.CreateCommand();
                // Use double-quote identifier escaping
                countCmd.CommandText = $"SELECT COUNT(*) FROM \"{name.Replace("\"", "\"\"")}\"";
                countCmd.CommandTimeout = 5;
                entry.RowCount = (long)(countCmd.ExecuteScalar() ?? 0L);
            }
            catch
            {
                entry.RowCountFailed = true;
            }
            model.Tables.Add(entry);
        }

        return model;
    }

    private static object? Run(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText    = sql;
        cmd.CommandTimeout = 5;
        return cmd.ExecuteScalar();
    }

    private static long    PragmaLong(SqliteConnection conn, string pragma) =>
        Run(conn, $"PRAGMA {pragma}") is long l ? l : 0L;

    private static string? PragmaStr(SqliteConnection conn, string pragma) =>
        Run(conn, $"PRAGMA {pragma}") as string;

    private static string? Scalar(SqliteConnection conn, string sql) =>
        Run(conn, sql) as string;

    private static long CountSchema(SqliteConnection conn, string type) =>
        Run(conn, $"SELECT COUNT(*) FROM sqlite_master WHERE type='{type}' AND name NOT LIKE 'sqlite_%'") is long l ? l : 0L;
}
