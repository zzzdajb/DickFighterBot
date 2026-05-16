using System.Data.SQLite;

namespace DickFighterBot.DataBase;

public partial class DickFighterDataBase
{
    public async Task<bool> UpdateDickNickName(long userId, long groupId, string newNickName)
    {
        // 根据GroupNumber和DickBelongings修改NickName
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand(connection)
            {
                CommandText =
                    "UPDATE BasicInformation SET NickName = @NickName WHERE DickBelongings = @DickBelongings AND GroupNumber = @GroupNumber"
            };
            command.Parameters.AddWithValue("@NickName", newNickName);
            command.Parameters.AddWithValue("@DickBelongings", userId);
            command.Parameters.AddWithValue("@GroupNumber", groupId);

            // 执行更新操作
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            // 处理异常，例如记录错误日志
            Logger.Error($"数据库操作：更新牛子昵称时发生错误：{e.Message}");
            // 返回更新失败
            return false;
        }
    }

    public async Task<bool> UpdateDickEnergy(int energy, string guid)
    {
        // 根据GUID更新体力
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand(connection)
            {
                CommandText =
                    "UPDATE Energy SET EnergyLastUpdate=@EnergyLastUpdate,EnergyLastUpdateTime=@EnergyLastUpdateTime  WHERE DickGUID = @DickGUID"
            };
            command.Parameters.AddWithValue("@EnergyLastUpdate", Math.Clamp(energy, 0, 240));
            command.Parameters.AddWithValue("@EnergyLastUpdateTime", DateTimeOffset.Now.ToUnixTimeSeconds());
            command.Parameters.AddWithValue("@DickGUID", guid);

            // 执行更新操作
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            // 处理异常，记录错误日志
            Logger.Error($"数据库操作：更新牛子体力时发生错误：{e.Message}");

            // 返回更新失败
            return false;
        }
    }

    public async Task<bool> UpdateDickLength(double length, string guid)
    {
        // 根据GUID更新体力
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand(connection)
            {
                CommandText =
                    "UPDATE BasicInformation SET Length=@Length  WHERE GUID = @GUID"
            };
            command.Parameters.AddWithValue("@Length", length);
            command.Parameters.AddWithValue("@GUID", guid);


            // 执行更新操作
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            // 处理异常，记录错误日志
            Logger.Error($"数据库操作：更新牛子长度时发生错误：{e.Message}");
            // 返回更新失败
            return false;
        }
    }
}