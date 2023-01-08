namespace IchieBotData.Effects;

public abstract class EffectInst
{
    public string Target { get; set; }
    public int Accuracy { get; set; }
    public List<int> Magnitudes { get; set; }
    public List<int> Amplitudes { get; set; }

    protected EffectInst(string target, int accuracy, List<int> magnitudes, List<int> amplitudes)
    {
        Target = target;
        Accuracy = accuracy;
        Magnitudes = magnitudes;
        Amplitudes = amplitudes;
    }
}