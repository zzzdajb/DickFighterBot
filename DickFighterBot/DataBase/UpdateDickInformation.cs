using System.Data.SQLite;
using Dapper;

namespace DickFighterBot.DataBase;

public partial class DickFighterDataBase
{
    public async Task<bool> UpdateDickNickName(long userId, long groupId, string newNickName)
    {
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();

            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE BasicInformation SET NickName = @NickName WHERE DickBelongings = @DickBelongings AND GroupNumber = @GroupNumber",
                new { NickName = newNickName, DickBelongings = userId, GroupNumber = groupId });

            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            Logger.Error($"数据库操作：更新牛子昵称时发生错误：{e.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateDickEnergy(int energy, string guid)
    {
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();

            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE Energy SET EnergyLastUpdate=@EnergyLastUpdate,EnergyLastUpdateTime=@EnergyLastUpdateTime WHERE DickGUID = @DickGUID",
                new
                {
                    EnergyLastUpdate = Math.Clamp(energy, 0, 240),
                    EnergyLastUpdateTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    DickGUID = guid
                });

            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            Logger.Error($"数据库操作：更新牛子体力时发生错误：{e.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateDickLength(double length, string guid)
    {
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();

            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE BasicInformation SET Length=@Length WHERE GUID = @GUID",
                new { Length = length, GUID = guid });

            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            Logger.Error($"数据库操作：更新牛子长度时发生错误：{e.Message}");
            return false;
        }
    }
}
