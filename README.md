# 斗战胜牛

![DickFighterBot](https://socialify.git.ci/zzzdajb/DickFighterBot/image?font=Inter&language=1&name=1&owner=1&pattern=Charlie+Brown&stargazers=1&theme=Auto)

#### 给本仓库 Star 的小伙伴，牛子都会增长 5cm！

---

## 依赖

- [.NET 8.0+](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [go-cqhttp](https://github.com/Mrs4s/go-cqhttp)（或其他 OneBot 兼容的 QQ 框架）
- SQLite（由 System.Data.SQLite 自动管理）

## 配置

1. 确保 go-cqhttp 已配置好正向 WebSocket，监听地址和端口在下一步配置。

2. 首次运行后，程序会在 `~/DickFighterBot/main.json` 生成默认配置文件（或手动创建）：

```json
{
  "MainSettings": {
    "ws_host": "127.0.0.1",
    "port": 3001,
    "Interval": 5000
  },
  "DickData": {
    "FightEnergyCost": 40,
    "ExerciseEnergyCost": 10
  },
  "Rank": {
    "GroupRankTopCount": 3,
    "GlobalRankTopCount": 3
  },
  "Management": {
    "Administrator": 393098870
  }
}
```

| 配置项 | 说明 |
|--------|------|
| `ws_host` | go-cqhttp WebSocket 地址 |
| `port` | go-cqhttp WebSocket 端口 |
| `Interval` | 消息发送间隔（ms） |
| `FightEnergyCost` | 每次斗牛消耗体力 |
| `ExerciseEnergyCost` | 每次锻炼消耗体力 |
| `GroupRankTopCount` | 群牛子榜显示名次 |
| `GlobalRankTopCount` | 全服牛子榜显示名次 |
| `Administrator` | 管理员 QQ 号（有权使用补偿指令） |

3. 运行：

```bash
dotnet run
```

## 指令列表

### 基础

| 指令 | 说明 |
|------|------|
| `牛子帮助` | 显示帮助信息 |
| `生成牛子` | 生成一只新牛子，初始长度 5~15cm，送 240 体力 |
| `我的牛子` | 查看牛子长度、体力、群内和全服排名 |
| `/status` | 查看机器人运行时间 |

### 牛子养成

| 指令 | 说明 |
|------|------|
| `锻炼牛子X次` | 消耗体力锻炼，可能增加或减少长度，例：`锻炼牛子5次` |
| `改牛子名 新名字` | 修改牛子昵称，最长 30 字 |
| `牛子咖啡` | 回复 60 点体力，每 20 小时限饮一次 |

### 对战

| 指令 | 说明 |
|------|------|
| `斗牛` | 消耗体力在全服随机匹配对手进行牛子 PK，胜方获得长度 |
| `真理牛子` | 消耗 60 体力对随机牛子发动追加攻击，命中则对方长度被取对数，自己获得收益 |

### 排行

| 指令 | 说明 |
|------|------|
| `群牛子榜` | 显示本群最长/最短牛子排行 |
| `全服牛子榜` | 显示全服最长/最短牛子排行 |

### 管理员

| 指令 | 说明 |
|------|------|
| `补偿体力` | 为当前群所有玩家补偿 240 点体力，仅配置文件中的 `Administrator` 可用 |

## 体力系统

- 体力上限 **240** 点
- 每 **6 分钟** 自动恢复 1 点（每小时 10 点）
- 各项操作消耗体力见配置文件

## 数据库

SQLite 数据库文件位于 `~/DickFighterBot/dickfightdatabase.db`。程序启动时自动建表并执行 Schema 迁移，无需手动管理。
