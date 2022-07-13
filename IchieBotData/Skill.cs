using IchieBotData.Common;
using IchieBotData.Effects;

namespace IchieBotData;

public class Skill
{
    public List<EffectInst> Effects { get; set; }
    public Condition Condition { get; set; }

    public Skill(List<EffectInst> effects, Condition condition)
    {
        Effects = effects;
        Condition = condition;
    }
}