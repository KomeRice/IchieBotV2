using IchieBotData.Common;

namespace IchieBotData.Effects;

public class DamageEffectInst : EffectInst
{
    public int HitCount { get; set; }
    public Element Element { get; set; }
    public DamageEffect Effect;

    public DamageEffectInst(DamageEffect effect, string target, int accuracy, List<int> amplitudes, List<int> magnitudes, int hitCount, Element element) : base(target, accuracy, magnitudes, amplitudes)
    {
        Effect = effect;
        HitCount = hitCount;
        Element = element;
    }
}