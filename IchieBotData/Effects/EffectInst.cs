using IchieBotData.Common;

namespace IchieBotData.Effects;

public abstract class EffectInst
{
    public string Target { get; set; }
    public int Accuracy { get; set; }
    public List<int> Magnitudes { get; set; }
    public List<int> Amplitudes { get; set; }
    public Element Element { get; set; }

    protected EffectInst(string target, int accuracy, List<int> magnitudes, List<int> amplitudes)
    {
        Target = target;
        Accuracy = accuracy;
        Magnitudes = magnitudes;
        Amplitudes = amplitudes;
    }

    public abstract string Description(bool shortString = true, bool firstLevel = false);

    public abstract string ExtraDescription();

    public abstract Effect GetEffect();
}