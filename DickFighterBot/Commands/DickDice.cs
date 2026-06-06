using DickFighterBot.config;
using DickFighterBot.DataBase;
using DickFighterBot.PublicAPI;
using DickFighterBot.Tools;

namespace DickFighterBot.Commands;

public class DickDice
{
    public async Task Roll(long user_id, long group_id, string rawMessage)
    {
        var parts = rawMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !double.TryParse(parts[1], out var betAmount) || betAmount <= 0)
        {
            await WebSocketClient.Send(群消息序列化工具.Generate(
                $"[CQ:at,qq={user_id}]，请输入有效的赌注长度！格式：牛子骰子 <长度>，例如：牛子骰子 50", group_id));
            return;
        }

        var dick = new Dick.Dick { Belongings = user_id, GroupNumber = group_id };
        if (!await dick.LoadWithIds())
        {
            await WebSocketClient.Send(群消息序列化工具.Generate(TipsMessage.DickNotFound(user_id), group_id));
            return;
        }

        //检查体力
        var energyCost = ConfigLoader.Load().DickData.FightEnergyCost;
        var dickFighterDataBase = new DickFighterDataBase();
        var currentEnergy = await dickFighterDataBase.CheckDickEnergyWithGuid(dick.GUID);
        if (currentEnergy < energyCost)
        {
            await WebSocketClient.Send(群消息序列化工具.Generate(
                TipsMessage.EnergyNotEnough(currentEnergy, energyCost, user_id), group_id));
            return;
        }

        if (dick.Length <= 0)
        {
            await WebSocketClient.Send(群消息序列化工具.Generate(
                $"[CQ:at,qq={user_id}]，你的牛子[{dick.NickName}]长度为{dick.Length:F1}cm，没法玩骰子！", group_id));
            return;
        }

        //扣体力
        dick.Energy = currentEnergy - energyCost;
        await dickFighterDataBase.UpdateDickEnergy(dick.Energy, dick.GUID);

        //赌注不能超过当前长度
        betAmount = Math.Min(betAmount, dick.Length);

        //骰子倍率分布：期望≈0.99，方差极大
        var roll = Random.Shared.NextDouble();
        double multiplier;
        string resultText;

        if (roll < 0.12) { multiplier = 0; resultText = "💀 全输！血本无归！"; }
        else if (roll < 0.35) { multiplier = 0.3; resultText = "😰 亏了七成..."; }
        else if (roll < 0.57) { multiplier = 0.5; resultText = "😅 亏了一半..."; }
        else if (roll < 0.75) { multiplier = 1.5; resultText = "🙂 小赚一笔！"; }
        else if (roll < 0.87) { multiplier = 2; resultText = "😊 翻倍了！"; }
        else if (roll < 0.94) { multiplier = 3; resultText = "🎉 三倍！赚大了！"; }
        else if (roll < 0.97) { multiplier = 5; resultText = "🔥 五倍！起飞！"; }
        else if (roll < 0.99) { multiplier = 10; resultText = "💎 十倍！超级大奖！"; }
        else { multiplier = 50; resultText = "🚀🚀🚀 五十倍！！！天选之牛！！！"; }

        var resultLength = betAmount * multiplier;
        var netChange = resultLength - betAmount;

        dick.Length += netChange;
        await new DickFighterDataBase().UpdateDickLength(dick.Length, dick.GUID);

        var changeWord = netChange >= 0 ? "赢了" : "亏了";
        var changeAbs = Math.Abs(netChange);
        var outputMessage =
            $"[CQ:at,qq={user_id}]，你花了{betAmount:F1}cm掷出了牛子骰子！\n" +
            $"🎲 {resultText}\n" +
            $"倍率：{multiplier}x | {changeWord}{changeAbs:F1}cm\n" +
            $"当前牛子[{dick.NickName}]长度：{dick.Length:F1}cm\n" +
            $"消耗体力{energyCost}，当前体力：{dick.Energy}/240";

        await WebSocketClient.Send(群消息序列化工具.Generate(outputMessage, group_id));
    }
}
