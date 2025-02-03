package dataBase

import java.nio.file.Files
import java.nio.file.Path
import java.nio.file.Paths
import java.nio.file.StandardOpenOption

class Initialize {

    fun main() {
        // 获取用户目录
        val userHome: Path = Paths.get(System.getProperty("user.home"))

        // 定义SQLite文件路径
        val sqliteFilePath: Path = userHome.resolve("dickFighterBot.db")

        // 创建SQLite文件
        if (!Files.exists(sqliteFilePath)) {
            Files.createFile(sqliteFilePath)
            println("未找到数据库，已于该位置创建新数据库: $sqliteFilePath")
        } else {
            println("数据库路径 $sqliteFilePath")
        }
    }
}