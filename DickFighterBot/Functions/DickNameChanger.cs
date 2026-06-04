using DickFighterBot.Commands;
using DickFighterBot.DataBase;
using DickFighterBot.PublicAPI;
using DickFighterBot.Tools;
using NLog;

namespace DickFighterBot.Functions;

public class DickNameChanger
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //获取日志记录器

    public static async Task Change(long user_id, long group_id, string rawMessage)
    {
        var (newName, ifNeedEdit) = 正则表达式.改牛子名(rawMessage);
        var dickFighterDataBase = new DickFighterDataBase();

        var newDick =
            await dickFighterDataBase.GetDickWithIds(user_id,
                group_id);

        if (ifNeedEdit)
        {
            string stringMessage;
            if (newDick != null)
            {
                //敏感词检查
                if (BadWordFilter.ContainsBadWord(newName))
                {
                    await WebSocketClient.Send(群消息序列化工具.Generate(
                        $"[CQ:at,qq={user_id}]，你取的什么破名字！牛子名字包含敏感词，请换一个！", group_id));
                    return;
                }

                //如果需要修改名字并且有牛子
                var changeResult = await dickFighterDataBase.UpdateDickNickName(user_id, group_id, newName);
                if (changeResult)
                {
                    stringMessage = $"[CQ:at,qq={user_id}]，你的牛子名字已经修改为[{newName}]！";
                    Logger.Info($"群{group_id}当中的用户{user_id}修改了牛子昵称，新昵称为：{newName}");
                }
                else
                {
                    stringMessage = $"[CQ:at,qq={user_id}]，你的牛子名字修改失败！请稍后再试！";
                    Logger.Error($"群{group_id}当中的用户{user_id}修改牛子昵称失败！");
                }
            }
            else
            {
                stringMessage = TipsMessage.DickNotFound(user_id);
            }

            await WebSocketClient.Send(群消息序列化工具.Generate(stringMessage, group_id));
        }
    }
}