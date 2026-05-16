using DickFighterBot.DataBase;
using DickFighterBot.PublicAPI;
using DickFighterBot.Tools;
using NLog;

namespace DickFighterBot.Commands;

public class Coffee
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public async Task DrinkCoffee(long user_id, long group_id)
    {
        string outputMessage;
        const int energyAdd = 60;
        var dickFighterDataBase = new DickFighterDataBase();
        var dick = await dickFighterDataBase.GetDickWithIds(user_id, group_id);

        if (dick != null)
        {
            var lastDrinkTimeFromDataBase =
                await dickFighterDataBase.CheckCoffeeInformation(dick.GUID);

            dick.Energy = await dickFighterDataBase.CheckDickEnergyWithGuid(dick.GUID);

            if (lastDrinkTimeFromDataBase == null)
            {
                await dickFighterDataBase.CreateNewCoffeeLine(dick.GUID);
                dick.Energy += energyAdd;
                await dickFighterDataBase.UpdateDickEnergy(dick.Energy, dick.GUID);

                Logger.Trace($"群{group_id}的用户{user_id}没有咖啡记录，新增一条咖啡记录！");

                outputMessage =
                    $"[CQ:at,qq={user_id}]，你的牛子[{dick.NickName}]饮用了一杯牛子咖啡，现在精神饱满，体力回复了{energyAdd}点。当前体力为{dick.Energy}/240。";
            }
            else
            {
                var lastDrinkTime = DateTimeOffset.FromUnixTimeSeconds(lastDrinkTimeFromDataBase.Value);
                var nextAvailableTime = lastDrinkTime.AddHours(20);
                var currentTime = DateTimeOffset.Now;

                if (nextAvailableTime < currentTime)
                {
                    await dickFighterDataBase.DrinkCoffee(dick.GUID);
                    dick.Energy += energyAdd;
                    await dickFighterDataBase.UpdateDickEnergy(dick.Energy, dick.GUID);

                    Logger.Trace($"群{group_id}的用户{user_id}饮用了一杯牛子咖啡。");

                    outputMessage =
                        $"[CQ:at,qq={user_id}]，你的牛子[{dick.NickName}]饮用了一杯牛子咖啡，现在精神饱满，体力回复了{energyAdd}点。当前体力为{dick.Energy}/240。";
                }
                else
                {
                    var restOfTime = nextAvailableTime - currentTime;
                    outputMessage =
                        $"[CQ:at,qq={user_id}]，你的牛子[{dick.NickName}]今天已经饮用过一杯咖啡了，请{restOfTime.Hours}小时{restOfTime.Minutes}分钟后再来！";
                }
            }
        }
        else
        {
            outputMessage = TipsMessage.DickNotFound(user_id);
        }

        await WebSocketClient.Send(群消息序列化工具.Generate(outputMessage, group_id));
    }
}
