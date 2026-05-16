using DickFighterBot.DataBase;
using NLog;

namespace DickFighterBot.Dick;

public partial class Dick
{
    private const int MaxEnergy = 240;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private int _energy;
    public long Belongings { get; set; } //牛子所属的QQ号
    public string? NickName { get; set; }
    public double Length { get; set; }
    public string? GUID { get; set; }
    public long GroupNumber { get; set; }

    private bool Loaded { get; set; }

    public int Energy
    {
        get => Math.Clamp(_energy, 0, MaxEnergy);
        set => _energy = Math.Clamp(value, 0, MaxEnergy);
    }

    public async Task LoadWithGuid()
    {
        var dickFighterDataBase = new DickFighterDataBase();
        if (GUID == null)
        {
            Logger.Fatal("在使用GUID加载牛子数据时，GUID不允许为空！");
            throw new Exception("GUID不能为空！");
        }

        var loaded = await dickFighterDataBase.CheckDickWithGuid(GUID);
        if (loaded != null)
        {
            Belongings = loaded.Belongings;
            NickName = loaded.NickName;
            Length = loaded.Length;
            GroupNumber = loaded.GroupNumber;

            //加载体力
            Energy = await dickFighterDataBase.CheckDickEnergyWithGuid(GUID);

            //加载成功
            Loaded = true;
        }
        else
        {
            Logger.Fatal("在使用GUID加载牛子数据时，GUID不存在！");
            throw new Exception("GUID不存在！");
        }
    }

    public async Task<bool> LoadWithIds()
    {
        var dickFighterDataBase = new DickFighterDataBase();
        GUID = await dickFighterDataBase.CheckGuidWithTwoIds(Belongings, GroupNumber);
        if (GUID != null)
        {
            await LoadWithGuid();
            return true;
        }

        Logger.Error("不存在的牛子！");
        return false;
    }

    private async Task Save()
    {
        var dickFighterDataBase = new DickFighterDataBase();
        await dickFighterDataBase.UpdateDickEnergy(Energy, GUID);
        await dickFighterDataBase.UpdateDickLength(Length, GUID);
        await dickFighterDataBase.UpdateDickNickName(Belongings, GroupNumber, NickName);
    }
}