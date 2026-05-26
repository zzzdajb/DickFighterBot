using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DickFighterBot.config;
using DickFighterBot.DataBase;
using NLog;

namespace DickFighterBot;

public class WebSocketClient
{
    private static ClientWebSocket websocketClient;

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //获取日志记录器

    public static async Task Main()
    {
        await DickFighterDataBase.Initialize(); //初始化数据库

        //加载配置文件
        var configFile = ConfigLoader.Load();
        var protocol = configFile.MainSettings.use_wss ? "wss" : "ws";
        var serverUri = new Uri($"{protocol}://{configFile.MainSettings.ws_host}:{configFile.MainSettings.port}");

        const int maxRetry = 10;
        const int retryDelayMs = 5000;

        for (var attempt = 1; attempt <= maxRetry; attempt++)
        {
            websocketClient = new ClientWebSocket();

            if (!string.IsNullOrEmpty(configFile.MainSettings.access_token))
            {
                websocketClient.Options.SetRequestHeader("Authorization",
                    $"Bearer {configFile.MainSettings.access_token}");
            }

            try
            {
                await websocketClient.ConnectAsync(serverUri, CancellationToken.None);
                Logger.Info("WebSocket服务器连接成功！");
                break;
            }
            catch (Exception ex)
            {
                websocketClient.Dispose();
                if (attempt == maxRetry)
                {
                    Logger.Fatal($"WebSocket连接失败（已重试{maxRetry}次）：" + ex.Message);
                    Logger.Fatal("错误详情：" + ex.StackTrace);
                    Logger.Info("按任意键退出程序。");
                    Console.ReadKey();
                    return;
                }

                Logger.Warn($"WebSocket连接失败（第{attempt}/{maxRetry}次重试）：{ex.Message}，{retryDelayMs / 1000}秒后重试...");
                await Task.Delay(retryDelayMs);
            }
        }

        try
        {
            // 启动消息接收任务，并等待其完成
            var receiveTask = Receive();
            await receiveTask;

            await websocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, "连接已关闭。",
                CancellationToken.None);
            Logger.Info("从WebSocket服务器断开连接！");
        }
        catch (Exception ex)
        {
            Logger.Fatal("主程序出现致命错误：" + ex.Message);
            Logger.Fatal("错误详情：" + ex.StackTrace);
            Logger.Info("按任意键退出程序。");
            Console.ReadKey();
        }
        finally
        {
            websocketClient.Dispose();
        }
    }

    public static async Task Send(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        await websocketClient.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true,
            CancellationToken.None);
        
    }

    private static async Task Receive()
    {
        var buffer = new byte[65536];
        var messageBuffer = new MemoryStream();
        while (websocketClient.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            messageBuffer.SetLength(0);
            do
            {
                result = await websocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                messageBuffer.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);

            var receivedMessage = Encoding.UTF8.GetString(messageBuffer.GetBuffer(), 0, (int)messageBuffer.Length);
            Logger.Trace("收到消息：" + receivedMessage);

            var dispatcher = new CommandDispatcher();

            try
            {
                var messageReceived = JsonSerializer.Deserialize<Message.GroupMessage>(receivedMessage);

                if (messageReceived is { user_id: > 0, group_id: > 0 })
                    await dispatcher.Dispatch(messageReceived.user_id, messageReceived.group_id, messageReceived);
            }
            catch (JsonException ex)
            {
                Logger.Warn($"解析JSON时出现异常：{ex.Message} ");
            }
        }
    }
}