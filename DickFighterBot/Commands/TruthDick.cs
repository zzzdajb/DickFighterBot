using DickFighterBot.DataBase;
using DickFighterBot.PublicAPI;
using DickFighterBot.Tools;

namespace DickFighterBot.Commands;

public class TruthDick
{
    //真理牛子可以将随机一只牛子的长度取对数，有效抑制膨胀

    public async Task 追加攻击(long user_id, long group_id, int energyCost = 60)
    {
        string outputMessage;

        var successRateList = new[] { 0.6, 0.8, 1.0 };
        var successRate = successRateList[Random.Shared.Next(0, successRateList.Length)];
        var success = Random.Shared.NextDouble() < successRate;
        var dickFighterDataBase = new DickFighterDataBase();

        var newDick = await dickFighterDataBase.GetDickWithIds(user_id, group_id);

        if (newDick != null)
        {
            var currentEnergy = await dickFighterDataBase.CheckDickEnergyWithGuid(newDick.GUID);
            newDick.Energy = currentEnergy;
            currentEnergy = newDick.Energy;

            if (currentEnergy >= energyCost)
            {
                await dickFighterDataBase.UpdateDickEnergy(guid: newDick.GUID, energy:
                    currentEnergy - energyCost);
                if (success)
                {
                    //随机抓出一只牛子，取对数
                    Dick.Dick? enemyDick;

                    var randomNumber = Random.Shared.NextDouble();
                    switch (randomNumber)
                    {
                        case < 1 / 3d:
                            enemyDick = await dickFighterDataBase.GetRandomDick(newDick.GUID);
                            break;
                        case < 1 / 2d:
                        {
                            var resultOfFirstNList = await dickFighterDataBase.GetFirstNDicksByOrder(1);
                            enemyDick = resultOfFirstNList[0];
                            break;
                        }
                        default:
                        {
                            var resultOfLastNList = await dickFighterDataBase.GetFirstNDicksByOrder(1, 1);
                            enemyDick = resultOfLastNList[0];
                            break;
                        }
                    }

                    var enemyOldLength = enemyDick.Length;
                    double newLength;

                    var pctForCalculate = 0.7d; //取对数的比例，只有这一部分会被取对数
                    var restPct = 1 - pctForCalculate;
                    if (enemyOldLength > 0)
                        newLength = Math.Log(enemyOldLength * pctForCalculate + 1) + restPct * enemyOldLength;
                    else
                        newLength = -Math.Log(Math.Abs(enemyOldLength * pctForCalculate) + 1) +
                                    restPct * enemyOldLength;

                    var lengthDifference = newLength - enemyOldLength;
                    enemyDick.Length = newLength;
                    await dickFighterDataBase.UpdateDickLength(newLength, enemyDick.GUID);

                    var winnerGet = -lengthDifference * RandomGenerator.GetRandomDouble(0.1, 0.2);
                    newDick.Length += winnerGet;
                    await dickFighterDataBase.UpdateDickLength(newDick.Length, newDick.GUID);

                    outputMessage =
                        $"[CQ:at,qq={user_id}]，你花费{energyCost}体力，尝试使用试用牛子“真理牛子”对全服的随机牛子发动追加攻击！" +
                        $"根据星际牛子公司测算，本次追加攻击发动的概率为{successRate * 100}%。天有不测风云，牛有旦夕祸福。{enemyDick.Belongings}的牛子[{enemyDick.NickName}]受到了你的攻击!该牛子的长度从{enemyOldLength:F2}cm变化为{newLength:F2}cm，长度变化为{lengthDifference:F2}cm。" +
                        $"在追加攻击发动的同时，你的牛子[{newDick.NickName}]掠夺了{winnerGet:F2}cm的长度，当前长度为{newDick.Length:F2}cm。";
                }
                else
                {
                    outputMessage =
                        $"[CQ:at,qq={user_id}]，你花费{energyCost}体力，尝试使用试用牛子“真理牛子”对全服的随机牛子发动追加攻击！" +
                        $"根据星际牛子公司测算，本次追加攻击发动的概率为{successRate * 100}%。天有不测风云，牛有旦夕祸福。然而，你的牛子的追加攻击并没有生效，全服没有任何牛子发生了变化，仅仅是你损失了一些体力而已。";
                }
            }
            else
            {
                outputMessage = TipsMessage.EnergyNotEnough(currentEnergy, energyCost, user_id);
            }
        }
        else
        {
            outputMessage = TipsMessage.DickNotFound(user_id);
        }

        await WebSocketClient.Send(群消息序列化工具.Generate(outputMessage, group_id));
    }
}