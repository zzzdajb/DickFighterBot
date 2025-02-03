import io.github.cdimascio.dotenv.dotenv
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.WebSocket
import okhttp3.WebSocketListener
import kotlin.system.exitProcess


fun main() {
    //导入环境变量和.env文件
    val env = dotenv()

    // 导入服务端配置
    val serverHost = env["ONEBOT_SERVER_HOST"]
    val serverPort = env["ONEBOT_SERVER_PORT"]?.toInt()
    val apiPath = "/api"
    val eventPath = "/event"
    val enableTLS: Boolean = env["ONEBOT_SERVER_ENABLE_TLS"].toBooleanStrictOrNull() ?: false
    val accessToken = env["ONEBOT_SERVER_TOKEN"]

    println("当前连接信息：\nOneBot WebSocket服务端地址: $serverHost:$serverPort, TLS启用状况: $enableTLS,\n访问令牌: $accessToken")

    val toolsInstance = tools()

    // 在既没有启用TLS也不是本地地址时，提示用户
    if (!enableTLS && !toolsInstance.isLocalAddr(serverHost)) {
        println(
            "警告：服务端地址可能不是本地地址，未启用TLS时，您的数据将会在没有任何加密的情况下传输。\n" +
                    "公网明文传输WebSocket存在遭受重放攻击的风险，这不仅有可能导致你的数据泄露，还有可能导致OneBot后端遭受他人控制。为了确保安全，请使用TLS+AccessToken以确保公网传输的安全性。" +
                    "\n如果你确认自己处于可信网络当中，是请输入y并回车以忽略警告继续运行，输入其他将自动关闭程序。"
        )

        val answer = readLine()
        if (answer?.lowercase() != "y") {
            println("连接已经取消，程序结束运行。")
            exitProcess(1)
        }
    }

    // 创建 OkHttp 客户端
    val client = OkHttpClient()

    // 创建 API 连接
    val apiUrl = if (enableTLS) "wss://$serverHost:$serverPort$apiPath" else "ws://$serverHost:$serverPort$apiPath"
    client.newWebSocket(
        Request.Builder()
            .url(apiUrl)
            .addHeader("Authorization", "Bearer $accessToken") // 添加 Authorization 头
            .build(),
        object : WebSocketListener() {
            override fun onOpen(webSocket: WebSocket, response: okhttp3.Response) {
                println("[API] Connected")
            }

            override fun onMessage(webSocket: WebSocket, text: String) {
                println("[API] Received: $text")
            }

            override fun onClosing(webSocket: WebSocket, code: Int, reason: String) {
                println("[API] Disconnected")
                webSocket.close(1000, null)
            }
        }
    )

    // 创建 Event 连接
    val eventUrl =
        if (enableTLS) "wss://$serverHost:$serverPort$eventPath" else "ws://$serverHost:$serverPort$eventPath"
    client.newWebSocket(
        Request.Builder()
            .url(eventUrl)
            .addHeader("Authorization", "Bearer $accessToken") // 添加 Authorization 头
            .build(),
        object : WebSocketListener() {
            override fun onOpen(webSocket: WebSocket, response: okhttp3.Response) {
                println("[Event] Connected")
            }

            override fun onMessage(webSocket: WebSocket, text: String) {
                println("[Event] Received: $text")
            }

            override fun onClosing(webSocket: WebSocket, code: Int, reason: String) {
                println("[Event] Disconnected")
                webSocket.close(1000, null)
            }
        }
    )

    // 保持程序运行
    Thread.sleep(Long.MAX_VALUE)
}