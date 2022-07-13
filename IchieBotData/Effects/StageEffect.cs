using IchieBotData.Common;

namespace IchieBotData.Effects;

public class StageEffect : NonDamageEffect
{
    public List<List<NonDamageEffectInst>> Effects { get; set; }

    public StageEffect(string name, string jpName, List<string> altNames, int iconId, string verbose, EffectQuality quality, EffectType type, string explanation, List<List<NonDamageEffectInst>> effects) : base(name, jpName, altNames, iconId, verbose, quality, type, explanation)
    {
        Effects = effects;
    }
}