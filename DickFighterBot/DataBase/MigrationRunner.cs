using System.Data.SQLite;
using Dapper;
using NLog;

namespace DickFighterBot.DataBase;

public static class MigrationRunner
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly List<(int Version, string Description, string Sql)> Migrations = new()
    {
        (0, "Create initial tables", """
                                    CREATE TABLE IF NOT EXISTS BasicInformation (
                                        GUID TEXT PRIMARY KEY,
                                        DickBelongings INTEGER,
                                        NickName TEXT,
                                        Length REAL,
                                        GroupNumber INTEGER
                                    );
                                    CREATE TABLE IF NOT EXISTS Energy (
                                        DickGUID TEXT PRIMARY KEY,
                                        EnergyLastUpdate INTEGER,
                                        EnergyLastUpdateTime INTEGER
                                    );
                                    CREATE TABLE IF NOT EXISTS CoffeeInformation (
                                        GUID TEXT PRIMARY KEY,
                                        LastDrinkTime INTEGER
                                    );
                                    """),
        (1, "Drop deprecated Gender column", "ALTER TABLE BasicInformation DROP COLUMN Gender"),
        (2, "Drop deprecated tables", """
                                       DROP TABLE IF EXISTS GachaInformation;
                                       DROP TABLE IF EXISTS ExerciseRecord;
                                       DROP TABLE IF EXISTS BattleRecord;
                                       """)
    };

    public static async Task RunAsync(SQLiteConnection connection)
    {
        await connection.ExecuteAsync(
            "CREATE TABLE IF NOT EXISTS SchemaVersion (Version INTEGER NOT NULL)");

        var currentVersion = await connection.QueryFirstOrDefaultAsync<int>(
            "SELECT COALESCE(MAX(Version), -1) FROM SchemaVersion");

        foreach (var m in Migrations.Where(m => m.Version > currentVersion).OrderBy(m => m.Version))
        {
            Logger.Info($"正在执行迁移 [{m.Version}]：{m.Description}");
            using var txn = await connection.BeginTransactionAsync();
            try
            {
                var statements = m.Sql.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                Logger.Info($"  共 {statements.Length} 条SQL语句");
                foreach (var stmt in statements)
                {
                    Logger.Trace($"  SQL: {stmt[..Math.Min(stmt.Length, 80)]}...");
                    await connection.ExecuteAsync(stmt, transaction: txn);
                }

                await connection.ExecuteAsync(
                    "INSERT INTO SchemaVersion (Version) VALUES (@Version)",
                    new { Version = m.Version }, txn);
                await txn.CommitAsync();
            }
            catch (SQLiteException ex) when (m.Version == 1)
            {
                Logger.Info($"  Gender 列不存在，跳过 ({ex.Message})");
                await txn.RollbackAsync();
                using var txn2 = await connection.BeginTransactionAsync();
                await connection.ExecuteAsync(
                    "INSERT INTO SchemaVersion (Version) VALUES (@Version)",
                    new { Version = m.Version }, txn2);
                await txn2.CommitAsync();
            }

            Logger.Info($"  迁移 [{m.Version}] 完成");
        }
    }
}
