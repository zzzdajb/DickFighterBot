using System.Text.Json;
using NLog;

namespace DickFighterBot.config;

public class ConfigLoader
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); //获取日志记录器

    private static string? _path;

    private static string LocalProgramPath
    {
        get
        {
            if (_path != null) return _path;
            _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "DickFighterBot");
            return _path;
        }
    }

    public static Config Load()
    {
        //这里判断配置文件路径要从两个方面看，首先检测是否有本地配置文件，如果没有则使用程序自带的配置文件
        var configName = "main.json";
        var configPath = Path.Combine(LocalProgramPath, configName);
        if (!File.Exists(configPath))
        {
            var currentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "config");
            var defaultConfigPath = Path.Combine(currentDirectory, configName);
            Directory.CreateDirectory(LocalProgramPath);
            File.Copy(defaultConfigPath, configPath);
            Logger.Info("已将默认配置文件拷贝到本地：" + configPath);
        }
        else
        {
            Logger.Info("检测到本地配置文件，已加载！");
        }

        var jsonString = File.ReadAllText(configPath);
        var programConfig = JsonSerializer.Deserialize<Config>(jsonString);

        if (programConfig != null) return programConfig;

        Logger.Fatal("读取配置文件时出现问题！");
        throw new Exception($"配置文件读取错误，请检查配置文件{configName}是否存在！");
    }

    public class Config
    {
        public MainSettings MainSettings { get; set; }
        public DickData DickData { get; set; }
        public Rank Rank { get; set; }

        public Management Management { get; set; }
    }

    public class MainSettings
    {
        public string ws_host { get; set; }
        public int port { get; set; }
        public string access_token { get; set; }
        public bool use_wss { get; set; }
        public int Interval { get; set; }
    }

    public class DickData
    {
        public int ExerciseEnergyCost { get; set; }
        public int FightEnergyCost { get; set; }
    }

    public class Rank
    {
        public int GroupRankTopCount { get; set; }
        public int GlobalRankTopCount { get; set; }
    }

    public class Management
    {
        public long Administrator { get; set; }
    }
}