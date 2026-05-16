using DickFighterBot.DataBase;
using DickFighterBot.PublicAPI;
using DickFighterBot.Tools;

namespace DickFighterBot.Commands;

public class DickChecker
{
    public async Task CheckSelfDick(long user_id, long group_id, Message.GroupMessage message)
    {
        string stringMessage;
        var dickFighterDataBase = new DickFighterDataBase();

        var newDick =
            await dickFighterDataBase.GetDickWithIds(user_id,
                group_id);

        if (newDick != null)
        {
            newDick.Energy = await dickFighterDataBase.CheckDickEnergyWithGuid(newDick.GUID);

            var ranks = await dickFighterDataBase.GetLengthRanks(newDick.GUID, group_id);

            stringMessage = "基本信息：\n" +
                            $"[CQ:at,qq={user_id}]，你的牛子“{newDick.NickName}”，目前长度为{newDick.Length:F1}cm，当前体力状况：[{newDick.Energy}/240]\n" +
                            "排名信息：\n" +
                            $"牛子群内排名：[{ranks.GroupRank}/{ranks.GroupTotal}]名；牛子全服排名：[{ranks.GlobalRank}/{ranks.GlobalTotal}]名";
        }
        else
        {
            stringMessage = TipsMessage.DickNotFound(user_id);
        }

        await WebSocketClient.Send(群消息序列化工具.Generate(stringMessage, group_id));
    }
}
