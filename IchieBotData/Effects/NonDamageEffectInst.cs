namespace IchieBotData.Effects;

public class NonDamageEffectInst : EffectInst
{
    public int Amplitude { get; set; }
    public string EffectName { get; set; }

    public NonDamageEffectInst(string target, int accuracy, int magntiude, List<string> optionsList, int amplitude, string effectName) : base(target, accuracy, magntiude, optionsList)
    {
        Amplitude = amplitude;
        EffectName = effectName;
    }
}