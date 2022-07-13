namespace IchieBotData.Effects;

public abstract class EffectInst
{
    public string Target { get; set; }
    public int Accuracy { get; set; }
    public int Magntiude { get; set; }
    public List<string> optionsList { get; set; }

    protected EffectInst(string target, int accuracy, int magntiude, List<string> optionsList)
    {
        Target = target;
        Accuracy = accuracy;
        Magntiude = magntiude;
        this.optionsList = optionsList;
    }
}