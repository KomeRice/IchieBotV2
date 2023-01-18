using IchieBotData.Common;

namespace IchieBotData.Effects;

public class NonDamageEffectInst : EffectInst
{
    public NonDamageEffect Effect;
    
    public NonDamageEffectInst(NonDamageEffect effect, string target, int accuracy, List<int> magnitudes, List<int> amplitudes, Element attribute) : base(target, accuracy, magnitudes, amplitudes)
    {
        Effect = effect;
        Element = attribute;
    }

    public override string Description(bool shortString = true, bool firstLevel = false)
    {
        var verbose = Effect.Verbose;
        if (shortString)
        {
            verbose = verbose.Replace("%value%", firstLevel ? 
                Magnitudes.First().ToString() : Magnitudes.Last().ToString());
        }
        else
        {
            verbose = verbose.Replace("%value%", $"({Magnitudes.First()}-{Magnitudes.Last()})");
        }
        
        verbose = verbose.Replace("%attr%", Element.ToString());

        if (Amplitudes.Count > 0 && Amplitudes.Max() != 0)
        {
            switch (Effect.Type)
            {
                case EffectType.Timed:
                    if(firstLevel)
                        verbose += shortString ? $" ({Amplitudes.First()}t)" : $" ({Amplitudes.First()}-{Amplitudes.Last()}t)";
                    else
                    {
                        verbose += shortString ? $" ({Amplitudes.Last()}t)" : $" ({Amplitudes.First()}-{Amplitudes.Last()}t)";
                    }
                    break;
                case EffectType.Stack:
                    if (firstLevel)
                    {
                        verbose += shortString
                            ? $" ({Amplitudes.First()} "
                            : $" ({Amplitudes.First()}-{Amplitudes.Last()} ";
                        verbose += Amplitudes.First() > 1 ? "Stacks)" : "Stack)";
                    }
                    else
                    {
                        verbose += shortString ? $" ({Amplitudes.Last()} " : $" ({Amplitudes.First()}-{Amplitudes.Last()} "; 
                        verbose += Amplitudes.Last() > 1 ? "Stacks)" : "Stack)";
                    }
                    break;
            }
        }

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