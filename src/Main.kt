import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.WebSocket
import okhttp3.WebSocketListener
import java.net.InetAddress
import kotlin.system.exitProcess

fun main() {
    // 定义服务端配置
    val serverHost = "localhost"
    val serverPort = 3001
    val apiPath = "/api"
    val eventPath = "/event"
    val enableTLS = false

    println("WebSocket 服务端地址: $serverHost:$serverPort")

    // 检查是否为本地地址
    fun isLocalAddr(host: String): Boolean {
        return try {
            val address = InetAddress.getByName(host)
            address.isLoopbackAddress || host.equals("localhost", ignoreCase = true)
        } catch (e: Exception) {
            false
        }
    }

    // 安全警告检查
    if (!enableTLS && !isLocalAddr(serverHost)) {
        println("警告：WebSocket服务端地址可能不是本地地址，在没有使用TLS证书的情况下，公网明文传输数据非常危险！")
        println("请确认是否要在没有启用TLS加密的情况下继续连接，是请输入 y，输入其他将自动关闭程序。")

        val answer = readLine()
        if (answer?.lowercase() != "y") {
            println("连接已经取消，程序结束运行。")
            exitProcess(1)
        }
    }

    // 本地地址提示
    if (isLocalAddr(serverHost)) {
        println("当前暂未启用TLS，您应使用并且只能使用本地地址。")
    } else {
        println("警告：WebSocket服务端地址可能不是本地地址，在没有使用TLS证书的情况下，公网明文传输数据非常危险！")
    }

    // 创建 OkHttp 客户端
    val client = OkHttpClient()

    // 创建 API 连接
    val apiUrl = if (enableTLS) "wss://$serverHost:$serverPort$apiPath" else "ws://$serverHost:$serverPort$apiPath"
    client.newWebSocket(
        Request.Builder().url(apiUrl).build(),
        object : WebSocketListener() {
            override fun onOpen(webSocket: WebSocket, response: okhttp3.Response) {
                println("[API] Connected")
            }

            override fun onMessage(webSocket: WebSocket, text: String) {
                println("[API] Received: $text")
            }

            override fun onClosing(webSocket: WebSocket, code: Int, reason: String) {
                println("[API] Disconnected")
            }
        }
    )

    // 创建 Event 连接
    val eventUrl = if (enableTLS) "wss://$serverHost:$serverPort$eventPath" else "ws://$serverHost:$serverPort$eventPath"
    client.newWebSocket(
        Request.Builder().url(eventUrl).build(),
        object : WebSocketListener() {
            override fun onOpen(webSocket: WebSocket, response: okhttp3.Response) {
                println("[Event] Connected")
            }

            override fun onMessage(webSocket: WebSocket, text: String) {
                println("[Event] Received: $text")
            }

            override fun onClosing(webSocket: WebSocket, code: Int, reason: String) {
                println("[Event] Disconnected")
            }
        }
    )

    // 保持程序运行
    Thread.sleep(Long.MAX_VALUE)
}