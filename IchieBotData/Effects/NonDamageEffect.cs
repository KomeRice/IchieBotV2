using IchieBotData.Common;

namespace IchieBotData.Effects;

public class NonDamageEffect : Effect
{
    public EffectQuality Quality { get; set; }
    public EffectType Type { get; set; }
    public string Explanation { get; set; }

    public NonDamageEffect(int id, string name, string jpName, List<string> altNames, int iconId, string verbose, EffectQuality quality, EffectType type, string explanation, string extraVerbose)
        : base(id, name, jpName, altNames, iconId, verbose, extraVerbose)
    {
        Quality = quality;
        Type = type;
        Explanation = explanation;
    }
}