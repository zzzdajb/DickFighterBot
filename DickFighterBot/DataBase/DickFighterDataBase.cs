using System.Data.SQLite;
using Dapper;
using NLog;

namespace DickFighterBot.DataBase;

public partial class DickFighterDataBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static async Task Initialize()
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();
        await MigrationRunner.RunAsync(connection);
    }

    public async Task<bool> GenerateNewDick(long userId, long groupId, Dick.Dick newDick)
    {
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();
            using var txn = await connection.BeginTransactionAsync();

            await connection.ExecuteAsync(
                "INSERT INTO BasicInformation (GUID, DickBelongings, NickName, Length, GroupNumber) " +
                "VALUES (@GUID, @DickBelongings, @NickName, @Length, @GroupNumber)",
                new
                {
                    GUID = newDick.GUID, DickBelongings = userId, NickName = "软弱牛子", Length = newDick.Length,
                    GroupNumber = groupId
                }, txn);

            await connection.ExecuteAsync(
                "INSERT INTO Energy (DickGUID, EnergyLastUpdate, EnergyLastUpdateTime) " +
                "VALUES (@DickGUID, @EnergyLastUpdate, @EnergyLastUpdateTime)",
                new
                {
                    DickGUID = newDick.GUID, EnergyLastUpdate = 240,
                    EnergyLastUpdateTime = DateTimeOffset.Now.ToUnixTimeSeconds()
                }, txn);

            await txn.CommitAsync();
            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"数据库操作：生成新牛子时发生错误：{e.Message}");
            return false;
        }
    }

    public async Task<Dick.Dick?> GetRandomDick(long groupid, string guid)
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<Dick.Dick>(
            "SELECT GUID, DickBelongings AS Belongings, NickName, Length FROM BasicInformation " +
            "WHERE GroupNumber = @GroupNumber AND GUID != @ExcludedGuid ORDER BY RANDOM() LIMIT 1",
            new { GroupNumber = groupid, ExcludedGuid = guid });
    }

    public async Task<Dick.Dick?> GetRandomDick(string guid)
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<Dick.Dick>(
            "SELECT GUID, DickBelongings AS Belongings, NickName, Length FROM BasicInformation " +
            "WHERE GUID != @ExcludedGuid ORDER BY RANDOM() LIMIT 1",
            new { ExcludedGuid = guid });
    }

    public async Task<Dick.Dick?> GetRandomDick(string guid, long userId)
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<Dick.Dick>(
            "SELECT GUID, DickBelongings AS Belongings, NickName, Length FROM BasicInformation " +
            "WHERE GUID != @ExcludedGuid AND DickBelongings != @UserId ORDER BY RANDOM() LIMIT 1",
            new { ExcludedGuid = guid, UserId = userId });
    }

    public async Task<RankInfo> GetLengthRanks(string guid, long groupNumber)
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        var length = await connection.ExecuteScalarAsync<double>(
            "SELECT COALESCE(Length, 0) FROM BasicInformation WHERE GUID = @GUID", new { GUID = guid });

        if (length == 0)
        {
            await connection.ExecuteAsync(
                "UPDATE BasicInformation SET Length = 0 WHERE GUID = @GUID AND Length IS NULL",
                new { GUID = guid });
        }

        var globalRank = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) + 1 FROM BasicInformation WHERE Length > @Length", new { Length = length });

        var globalTotal = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM BasicInformation");

        var groupRank = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) + 1 FROM BasicInformation WHERE GroupNumber = @GroupNumber AND Length > @Length",
            new { GroupNumber = groupNumber, Length = length });

        var groupTotal = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM BasicInformation WHERE GroupNumber = @GroupNumber",
            new { GroupNumber = groupNumber });

        return new RankInfo(globalRank, globalTotal, groupRank, groupTotal);
    }
}
