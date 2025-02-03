import java.net.InetAddress

class tools {
    //一个静态类，提供一些工具方法

    //判断host是否为本地地址
    fun isLocalAddr(host: String): Boolean {
        return try {
            val address = InetAddress.getByName(host)
            address.isLoopbackAddress || host.equals("localhost", ignoreCase = true)
        } catch (e: Exception) {
            false
        }
    }
}