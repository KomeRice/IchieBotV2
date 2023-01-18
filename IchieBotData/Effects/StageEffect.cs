using IchieBotData.Common;

namespace IchieBotData.Effects;

public class StageEffect : NonDamageEffect
{
    public List<List<NonDamageEffectInst>> Effects { get; set; }

    public StageEffect(int id, string name, string jpName, List<string> altNames, int iconId, string verbose, EffectQuality quality, EffectType type, string explanation, string extraVerbose, List<List<NonDamageEffectInst>> effects, List<string> tags)
        : base(id, name, jpName, altNames, iconId, verbose, quality, type, explanation, extraVerbose, tags)
    {
        Effects = effects;
    }
}