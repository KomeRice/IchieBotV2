using IchieBotData.Common;

namespace IchieBotData;

public class Dress
{
    public string DressId { get; set; }
    public string ThumbUrl { get; set; }
    public string Name { get; set; }
    public Element Element { get; set; }
    public List<List<int>> Stats { get; set; }
    public int RowIndex { get; set; }
    public AttackType Type { get; set; }
    public int ReleaseWW { get; set; }
    public int ReleaseJP { get; set; }
    public List<Act> BasicActs { get; set; }
    public List<Skill> AutoSkills { get; set; }
    public Act ClimaxAct { get; set; }
    public Skill? UnitSkill { get; set; }
    public Skill? EntrySkill { get; set; }
    public List<string> Aliases { get; set; }
    public int Cost { get; set; }
    public int BaseRarity { get; set; }
    public Pool Pool { get; set; }
    public string Notes { get; set; }
    public bool HasRemake { get; set; }

    public Dress(string dressId, string thumbUrl, string name, Element element, List<List<int>> stats, int rowIndex, AttackType type, int releaseWw, int releaseJp, List<Act> basicActs, List<Skill> autoSkills, Act climaxAct, Skill? unitSkill, Skill? entrySkill, List<string> aliases, int cost, int baseRarity, Pool pool, string notes, bool hasRemake)
    {
        DressId = dressId;
        ThumbUrl = thumbUrl;
        Name = name;
        Element = element;
        Stats = stats;
        RowIndex = rowIndex;
        Type = type;
        ReleaseWW = releaseWw;
        ReleaseJP = releaseJp;
        BasicActs = basicActs;
        AutoSkills = autoSkills;
        ClimaxAct = climaxAct;
        UnitSkill = unitSkill;
        EntrySkill = entrySkill;
        Aliases = aliases;
        Cost = cost;
        BaseRarity = baseRarity;
        Pool = pool;
        Notes = notes;
        HasRemake = hasRemake;
    }
}