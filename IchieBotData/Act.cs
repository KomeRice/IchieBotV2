namespace IchieBotData;

public class Act
{
    public string Name { get; set; }
    public Skill Skill { get; set; }
    public Skill? AltSkill { get; set; }
    public int Cost { get; set; }
    public Act(string name, Skill skill, Skill? altSkill, int cost)
    {
        Name = name;
        Skill = skill;
        this.AltSkill = altSkill;
        Cost = cost;
    }
}