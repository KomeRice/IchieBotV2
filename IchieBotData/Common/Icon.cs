namespace IchieBotData.Common;

public class Icon
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Emote { get; set; }
    
    public Icon(string id, string name, string emote)
    {
        Id = id;
        Name = name;
        Emote = emote;
    }
}