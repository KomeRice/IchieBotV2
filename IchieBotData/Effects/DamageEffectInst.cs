using IchieBotData.Common;

namespace IchieBotData.Effects;

public class DamageEffectInst : EffectInst
{
    public int HitCount { get; set; }
    public string EffectName { get; set; }
    public Element Element { get; set; }

    public DamageEffectInst(string target, int accuracy, int magntiude, List<string> optionsList, int hitCount, string effectName, Element element) : base(target, accuracy, magntiude, optionsList)
    {
        HitCount = hitCount;
        EffectName = effectName;
        Element = element;
    }
}