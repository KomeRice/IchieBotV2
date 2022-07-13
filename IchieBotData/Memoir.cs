namespace IchieBotData;

public class Memoir
{
    public List<List<int>> Stats { get; set; }
    public List<string> Characters { get; set; }
    public int Cost { get; set; }
    public int Rarity { get; set; }
    public List<Skill> Skills { get; set; }

    public Memoir(List<List<int>> stats, List<string> characters, int cost, int rarity, List<Skill> skills)
    {
        Stats = stats;
        Characters = characters;
        Cost = cost;
        Rarity = rarity;
        Skills = skills;
    }
}