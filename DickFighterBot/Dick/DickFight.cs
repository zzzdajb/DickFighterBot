using DickFighterBot.config;
using DickFighterBot.DataBase;
using DickFighterBot.Tools;

namespace DickFighterBot.Dick;

public partial class Dick
{
    public async Task<string> Fight()
    {
        var perCost = ConfigLoader.Load().DickData.FightEnergyCost;
        string outputMessage;

        await LoadWithGuid();

        if (Energy >= perCost)
        {
            Energy -= perCost;
            var dickFighterDataBase = new DickFighterDataBase();

            //随机匹配群内对手，但不包括自己
            var enemyDick = await dickFighterDataBase.GetRandomDick(GroupNumber, GUID);

            if (enemyDick == null) return "当前群内牛子数量不足，无法斗牛！快邀请群友一起生成牛子吧！";

            await enemyDick.LoadWithGuid(); //加载敌方牛子数据
            var result = FightCalculator.Calculate(Length, enemyDick.Length, Length - enemyDick.Length);

            var currentLength = Length;
            Length += result.challengerChange;

            var enemyCurrentLength = enemyDick.Length;
            enemyDick.Length += result.defenderChange;

            var stringMessage1 =
                $"[CQ:at,qq={Belongings}]，你的牛子[{NickName}]向[CQ:at,qq={enemyDick.Belongings}]的牛子[{enemyDick.NickName}]发起了斗牛！本次斗牛消耗了{perCost}点体力，据牛科院物理研究所推测，你的牛子[{NickName}]胜率为{result.winRatePct:F1}%。";
            var stringMessage2 = result.isWin
                ? 文案生成器.WinWhenFight(Belongings, NickName, enemyDick.Belongings, enemyDick.NickName)
                : 文案生成器.LoseWhenFight(Belongings, NickName, enemyDick.Belongings, enemyDick.NickName);

            var stringMessage3 =
                $"斗牛结束后，你的牛子[{NickName}]的长度由{currentLength:F1}cm变化为{Length:F1}cm，变化了{result.challengerChange:F2}cm；" +
                $"对方牛子[{enemyDick.NickName}]的长度由{enemyCurrentLength:F1}cm变化为{enemyDick.Length:F1}cm，变化了{result.defenderChange:F2}cm。";

            var stringMessage4 = $"\n目前，你的牛子体力值为{Energy}/{MaxEnergy}。";

            //保存数据
            await Save();
            await enemyDick.Save();

            outputMessage = stringMessage1 + stringMessage2 + stringMessage3 + stringMessage4;
            return outputMessage;
        }

        outputMessage = $"[CQ:at,qq={Belongings}] ,你都没有体力了，斗个√8毛！\n目前，你的牛子体力值为{Energy}/{MaxEnergy}。";
        return outputMessage;
    }

    public async Task<string> CrossGroupFight()
    {
        var perCost = ConfigLoader.Load().DickData.FightEnergyCost;
        string outputMessage;

        await LoadWithGuid();

        if (Energy >= perCost)
        {
            Energy -= perCost;
            var dickFighterDataBase = new DickFighterDataBase();

            //随机匹配全服对手，但不包括自己的所有牛子
            var enemyDick = await dickFighterDataBase.GetRandomDick(GUID, Belongings);

            if (enemyDick == null) return "当前服务器内牛子数量不足，无法跨服斗牛！";

            await enemyDick.LoadWithGuid();
            var result = FightCalculator.Calculate(Length, enemyDick.Length, Length - enemyDick.Length);

            var currentLength = Length;
            Length += result.challengerChange;

            var enemyCurrentLength = enemyDick.Length;
            enemyDick.Length += result.defenderChange;

            var stringMessage1 =
                $"[CQ:at,qq={Belongings}]，你的牛子[{NickName}]向{enemyDick.Belongings}的牛子[{enemyDick.NickName}]发起了跨服斗牛！本次斗牛消耗了{perCost}点体力，据牛科院物理研究所推测，你的牛子[{NickName}]胜率为{result.winRatePct:F1}%。";
            var stringMessage2 = result.isWin
                ? 文案生成器.WinWhenFight(Belongings, NickName, enemyDick.Belongings, enemyDick.NickName)
                : 文案生成器.LoseWhenFight(Belongings, NickName, enemyDick.Belongings, enemyDick.NickName);

            var stringMessage3 =
                $"斗牛结束后，你的牛子[{NickName}]的长度由{currentLength:F1}cm变化为{Length:F1}cm，变化了{result.challengerChange:F2}cm；" +
                $"对方牛子[{enemyDick.NickName}]的长度由{enemyCurrentLength:F1}cm变化为{enemyDick.Length:F1}cm，变化了{result.defenderChange:F2}cm。";

            var stringMessage4 = $"\n目前，你的牛子体力值为{Energy}/{MaxEnergy}。";

            await Save();
            await enemyDick.Save();

            outputMessage = stringMessage1 + stringMessage2 + stringMessage3 + stringMessage4;
            return outputMessage;
        }

        outputMessage = $"[CQ:at,qq={Belongings}] ,你都没有体力了，斗个√8毛！\n目前，你的牛子体力值为{Energy}/{MaxEnergy}。";
        return outputMessage;
    }
}