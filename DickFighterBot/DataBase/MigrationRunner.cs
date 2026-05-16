using System.Data.SQLite;
using Dapper;

namespace DickFighterBot.DataBase;

public static class MigrationRunner
{
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
        (1, "Drop deprecated Gender column", "ALTER TABLE BasicInformation DROP COLUMN IF EXISTS Gender"),
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
            "SELECT COALESCE(MAX(Version), 0) FROM SchemaVersion");

        foreach (var m in Migrations.Where(m => m.Version > currentVersion).OrderBy(m => m.Version))
        {
            using var txn = await connection.BeginTransactionAsync();
            await connection.ExecuteAsync(m.Sql, transaction: txn);
            await connection.ExecuteAsync(
                "INSERT INTO SchemaVersion (Version) VALUES (@Version)",
                new { Version = m.Version }, txn);
            await txn.CommitAsync();
        }
    }
}
