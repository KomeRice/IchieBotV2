namespace IchieBotData.Effects;

// TODO: Generate elemental damage increase / reduction effects
// TODO: Generate status effect resistance effects
// TODO: Check for flippers

public abstract class Effect
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string JpName { get; set; }
    public List<string> AltNames { get; set; }
    public int IconId { get; set; }
    public string Verbose { get; set; }
    public string ExtraVerbose { get; set; }
    
    public List<string> Tags { get; set; }

    protected Effect(int id, string name, string jpName, List<string> altNames, int iconId, string verbose, string extraVerbose, List<string> tags)
    {
        Id = id;
        Name = name;
        JpName = jpName;
        AltNames = altNames;
        IconId = iconId;
        Verbose = verbose;
        ExtraVerbose = extraVerbose;
        Tags = tags;
    }
}