using IchieBotData.Common;
using IchieBotData.Effects;

namespace IchieBotData;

public class CutInSkill : Skill
{
    public string Trigger { get; set; }
    public int Cost { get; set; }
    public int InitialCooldown { get; set; }
    public int NormalCooldown { get; set; }
    public int UsageLimit { get; set; }

    public CutInSkill(List<EffectInst> effects, Condition condition, string trigger, int cost, int initialCooldown, int normalCooldown, int usageLimit) : base(effects, condition)
    {
        Trigger = trigger;
        Cost = cost;
        InitialCooldown = initialCooldown;
        NormalCooldown = normalCooldown;
        UsageLimit = usageLimit;
    }
}