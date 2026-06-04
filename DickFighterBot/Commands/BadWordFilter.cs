using NLog;

namespace DickFighterBot.Commands;

public static class BadWordFilter
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly HashSet<string> BadWords = new(StringComparer.Ordinal);
    private static bool _loaded;

    //需要从昵称中剔除的词，这些词本身不是敏感词
    private static readonly string[] IgnoreWords = { "牛子", "牛" };

    private static void Load()
    {
        if (_loaded) return;

        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Sensitive-lexicon", "Vocabulary");
        if (!Directory.Exists(dir))
        {
            Logger.Warn("敏感词库目录不存在：{Dir}", dir);
            _loaded = true;
            return;
        }

        foreach (var file in Directory.GetFiles(dir, "*.txt"))
        {
            foreach (var line in File.ReadLines(file))
            {
                var word = line.Trim();
                if (word.Length > 0)
                    BadWords.Add(word);
            }
        }

        Logger.Info("已加载 {Count} 个敏感词", BadWords.Count);
        _loaded = true;
    }

    /// <summary>
    /// 检查昵称是否包含敏感词。返回true表示包含敏感词。
    /// </summary>
    public static bool ContainsBadWord(string nickname)
    {
        Load();

        //剔除"牛子"、"牛"后再检查
        var text = nickname;
        foreach (var word in IgnoreWords)
            text = text.Replace(word, "");

        if (text.Length == 0) return false;

        foreach (var badWord in BadWords)
        {
            if (text.Contains(badWord, StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
