using System.Data.SQLite;

namespace DickFighterBot.DataBase;

public partial class DickFighterDataBase
{
    public async Task<bool> Compensation(long group_id, int energyCompensate = 240)
    {
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();

            var command = new SQLiteCommand(connection)
            {
                CommandText =
                    "UPDATE Energy SET EnergyLastUpdate=MIN(EnergyLastUpdate+@energyCompensate, 240) WHERE DickGUID IN (SELECT GUID FROM BasicInformation WHERE GroupNumber=@GroupNumber)"
            };

            command.Parameters.AddWithValue("@GroupNumber", group_id);
            command.Parameters.AddWithValue("@energyCompensate", energyCompensate);

            var rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
        catch (Exception e)
        {
            Logger.Error("检测补偿数据是否存在时出现错误！");
            Logger.Error(e.Message);
            throw;
        }
    }
}