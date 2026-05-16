using System.Data.SQLite;
using Dapper;

namespace DickFighterBot.DataBase;

public partial class DickFighterDataBase
{
    public async Task<long?> CheckCoffeeInformation(string guid)
    {
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();

            return await connection.QueryFirstOrDefaultAsync<long?>(
                "SELECT LastDrinkTime FROM CoffeeInformation WHERE GUID = @GUID",
                new { GUID = guid });
        }
        catch (Exception e)
        {
            Logger.Error("检测咖啡数据是否存在时出现错误！");
            Logger.Error(e.Message);
            throw;
        }
    }

    public async Task<bool> CreateNewCoffeeLine(string guid)
    {
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();

            var rowsAffected = await connection.ExecuteAsync(
                "INSERT INTO CoffeeInformation (GUID,LastDrinkTime) VALUES (@GUID, @LastDrinkTime)",
                new { GUID = guid, LastDrinkTime = DateTimeOffset.Now.ToUnixTimeSeconds() });

            if (rowsAffected == 1) return true;

            Logger.Error("创建咖啡数据时出现错误！");
            return false;
        }
        catch (Exception e)
        {
            Logger.Error("检测咖啡数据是否存在时出现错误！");
            Logger.Error(e.Message);
            throw;
        }
    }

    public async Task<bool> DrinkCoffee(string guid)
    {
        try
        {
            await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
            await connection.OpenAsync();

            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE CoffeeInformation SET LastDrinkTime = @LastDrinkTime WHERE GUID = @GUID",
                new { GUID = guid, LastDrinkTime = DateTimeOffset.Now.ToUnixTimeSeconds() });

            if (rowsAffected == 1) return true;

            Logger.Error("饮用咖啡时出现错误！");
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
