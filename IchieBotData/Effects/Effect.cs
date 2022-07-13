namespace IchieBotData.Effects;

// TODO: Generate elemental damage increase / reduction effects
// TODO: Generate status effect resistance effects
// TODO: Check for flippers

public abstract class Effect
{
    public string Name { get; set; }
    public string JpName { get; set; }
    public List<string> AltNames { get; set; }
    public int IconId { get; set; }
    public string Verbose { get; set; }

    protected Effect(string name, string jpName, List<string> altNames, int iconId, string verbose)
    {
        Name = name;
        JpName = jpName;
        AltNames = altNames;
        IconId = iconId;
        Verbose = verbose;
    }
}