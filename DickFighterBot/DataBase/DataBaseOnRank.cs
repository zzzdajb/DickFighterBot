using System.Data.SQLite;
using Dapper;

namespace DickFighterBot.DataBase;

public partial class DickFighterDataBase
{
    public async Task<int> GetCountOfTotalDicks()
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM main.BasicInformation");
    }

    public async Task<int> GetCountOfTotalDicks(long group_id)
    {
        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM main.BasicInformation WHERE GroupNumber = @group_id",
            new { group_id });
    }

    public async Task<List<Dick.Dick>> GetFirstNDicksByOrder(int n, int order = 0)
    {
        await FixNullLengths();

        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        var sql = order switch
        {
            0 => "SELECT GUID, DickBelongings AS Belongings, NickName, Length FROM main.BasicInformation ORDER BY Length DESC LIMIT @n",
            1 => "SELECT GUID, DickBelongings AS Belongings, NickName, Length FROM main.BasicInformation ORDER BY Length LIMIT @n",
            _ => throw new ArgumentOutOfRangeException(nameof(order), "排序方式只能为0或1")
        };

        var list = await connection.QueryAsync<Dick.Dick>(sql, new { n });
        return list.AsList();
    }

    public async Task<List<Dick.Dick>> GetFirstNDicksByOrder(int n, long group_id, int order = 0)
    {
        await FixNullLengths();

        await using var connection = new SQLiteConnection(DatabaseConnectionManager.ConnectionString);
        await connection.OpenAsync();

        var sql = order switch
        {
            0 => "SELECT GUID, DickBelongings AS Belongings, NickName, Length FROM main.BasicInformation WHERE GroupNumber=@group_id ORDER BY Length DESC LIMIT @n",
            1 => "SELECT GUID, DickBelongings AS Belongings, NickName, Length FROM main.BasicInformation WHERE GroupNumber=@group_id ORDER BY Length LIMIT @n",
            _ => throw new ArgumentOutOfRangeException(nameof(order), "排序方式只能为0或1")
        };

        var list = await connection.QueryAsync<Dick.Dick>(sql, new { n, group_id });
        return list.AsList();
    }
}
