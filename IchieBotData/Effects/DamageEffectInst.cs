using System.Globalization;
using IchieBotData.Common;

namespace IchieBotData.Effects;

public class DamageEffectInst : EffectInst
{
    public int HitCount { get; set; }
    public DamageEffect Effect;

    public DamageEffectInst(DamageEffect effect, string target, int accuracy, List<int> amplitudes, List<int> magnitudes, int hitCount, Element element) : base(target, accuracy, magnitudes, amplitudes)
    {
        Effect = effect;
        HitCount = hitCount;
        Element = element;
    }

    public override string Description(bool shortString = true, bool firstLevel = false)
    {
        var verbose = Effect.Verbose;
        var dividedMagnitudes = Magnitudes.Select(i => Math.Round(i / (float) HitCount, 2)).ToList();
        if (shortString)
        {
            if (HitCount > 1)
            {
                verbose = Effect.Tags.Contains("fixedDamage") ? 
                    verbose.Replace("%value%", $"{HitCount}*{dividedMagnitudes.Last()} ({Magnitudes.Last()})") : 
                    verbose.Replace("%value%%", $"{HitCount}*{dividedMagnitudes.Last()}% ({Magnitudes.Last()}%)");
            }
            else
            {
                verbose = verbose.Replace("%value%", Magnitudes.Last().ToString(CultureInfo.InvariantCulture));
            }
        }
        else
        {
            if (HitCount > 1)
            {
                verbose = Effect.Tags.Contains("fixedDamage") ? 
                    verbose.Replace("%value%", $"{HitCount}*({dividedMagnitudes.First()}-{dividedMagnitudes.Last()}) ({Magnitudes.First()}-{Magnitudes.Last()})") : 
                    verbose.Replace("%value%%", $"{HitCount}*({dividedMagnitudes.First()}%-{dividedMagnitudes.Last()}%) ({Magnitudes.First()}%-{Magnitudes.Last()}%)");
            }
            else
            {
                verbose = verbose.Replace("%value%", Magnitudes.Last().ToString(CultureInfo.InvariantCulture));
            }
        }

        verbose = verbose.Replace("%attr%", Element.ToString());

        return verbose;
    }
    
    public override string ExtraDescription()
    {
        return Effect.ExtraVerbose;
    }

    public override Effect GetEffect()
    {
        return Effect;
    }
}