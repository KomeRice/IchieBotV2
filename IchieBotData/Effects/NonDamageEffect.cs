using IchieBotData.Common;

namespace IchieBotData.Effects;

public class NonDamageEffect : Effect
{
    public EffectQuality Quality { get; set; }
    public EffectType Type { get; set; }
    public string Explanation { get; set; }

    public NonDamageEffect(string name, string jpName, List<string> altNames, int iconId, string verbose, EffectQuality quality, EffectType type, string explanation) : base(name, jpName, altNames, iconId, verbose)
    {
        Quality = quality;
        Type = type;
        Explanation = explanation;
    }
}