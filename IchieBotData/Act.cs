namespace IchieBotData;

public class Act
{
    public string Name { get; set; }
    public int Icon { get; set; }
    public Skill Skill { get; set; }
    public Skill altSkill { get; set; }
    public int Cost { get; set; }

    public Act(string name, int icon, Skill skill, Skill altSkill, int cost)
    {
        Name = name;
        Icon = icon;
        Skill = skill;
        this.altSkill = altSkill;
        Cost = cost;
    }
}