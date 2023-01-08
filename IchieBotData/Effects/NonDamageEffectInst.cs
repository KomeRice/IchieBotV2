namespace IchieBotData.Effects;

public class NonDamageEffectInst : EffectInst
{
    public NonDamageEffect Effect;
    
    public NonDamageEffectInst(NonDamageEffect effect, string target, int accuracy, List<int> magnitudes, List<int> amplitudes) : base(target, accuracy, magnitudes, amplitudes)
    {
        Effect = effect;
    }
}