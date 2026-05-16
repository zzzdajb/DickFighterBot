using System.Data.SQLite;
using Dapper;

namespace DickFighterBot.DataBase;

public partial class DickFighterDataBase
{
    public async Task<Dick.Dick?> GetDickWithIds(long userId, long groupId)
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<Dick.Dick>(
            "SELECT GUID, DickBelongings AS Belongings, NickName, Length FROM BasicInformation WHERE DickBelongings = @DickBelongings AND GroupNumber = @GroupNumber",
            new { DickBelongings = userId, GroupNumber = groupId });
    }

    public async Task<string?> CheckGuidWithTwoIds(long userId, long groupId)
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<string>(
            "SELECT GUID FROM BasicInformation WHERE DickBelongings = @DickBelongings AND GroupNumber = @GroupNumber",
            new { DickBelongings = userId, GroupNumber = groupId });
    }

    public async Task<Dick.Dick?> CheckDickWithGuid(string guid)
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<Dick.Dick>(
            "SELECT GUID, DickBelongings AS Belongings, GroupNumber, NickName, Length FROM BasicInformation WHERE GUID = @guid",
            new { guid });
    }

    public async Task<int> CheckDickEnergyWithGuid(string guid)
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        var result = await connection.QueryFirstOrDefaultAsync<(long EnergyLastUpdate, long EnergyLastUpdateTime)>(
            "SELECT EnergyLastUpdate, EnergyLastUpdateTime FROM Energy WHERE DickGUID = @DickGUID",
            new { DickGUID = guid });

        var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        var timeDifference = currentTime - result.EnergyLastUpdateTime;
        return Convert.ToInt32(result.EnergyLastUpdate + timeDifference / (6 * 60));
    }

    private async Task<List<Dick.Dick>> CheckDickWithGroupId(long groupId)
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        var list = await connection.QueryAsync<Dick.Dick>(
            "SELECT GUID, DickBelongings AS Belongings, NickName, Length FROM BasicInformation WHERE GroupNumber = @GroupNumber",
            new { GroupNumber = groupId });

        return list.AsList();
    }
}
