using IchieBotData.Common;
using IchieBotData.Effects;

namespace IchieBotData;

public class Skill
{
    public List<EffectInst> Effects { get; set; }
    public Condition Condition { get; set; }
    public string Icon { get; set; }

    public Skill(List<EffectInst> effects, Condition condition, string iconId)
    {
        Effects = effects;
        Condition = condition;
        Icon = iconId;
    }
    
    // Some auto skills require this as `dress.json` does not hold specific attributes for them
    public Skill CloneWithElement(Element attribute)
    {
        var ret = new Skill(Effects, Condition, Icon);
        var newEffects = new List<EffectInst>();
        
        foreach (var effect in ret.Effects)
        {
            if (effect is NonDamageEffectInst)
            {
                var nde = (NonDamageEffectInst)effect;
                newEffects.Add(new NonDamageEffectInst(nde.Effect, nde.Target, nde.Accuracy,
                    nde.Magnitudes, nde.Amplitudes, attribute));
            }
            else
            {
                var de = (DamageEffectInst)effect;
                newEffects.Add(new DamageEffectInst(de.Effect, de.Target, de.Accuracy, de.Amplitudes, de.Magnitudes, de.HitCount, attribute));
            }
        }

        ret.Effects = newEffects;

        return ret;
    }
}