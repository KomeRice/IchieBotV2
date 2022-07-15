using System.Collections;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using IchieBotData.Common;
using Newtonsoft.Json;
using OldJsonFormatParser.LegacyClass;

namespace IchieBotV2.Services;

public class DatabaseService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private List<StageGirl> Dress { get; set; }
    private Dictionary<string,StageGirl> DressDict { get; set; }
    
    private Dictionary<string,string> IconsDict { get; set; }

    public DatabaseService(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
    {
        _client = client;
        _commands = commands;
        _services = services;
        DressDict = new Dictionary<string, StageGirl>();

        Dress = DeserializeJson<StageGirl>(@"Legacy/json/stagegirls.json");
        foreach(var d in Dress)
        {
            DressDict.Add(d.DressId[2..], d);
        }

        IconsDict = new Dictionary<string, string>();
        
        foreach (var i in DeserializeJson<Icon>(@"Data/Icons.json"))
        {
            IconsDict[i.Name] = i.Emote;
        }
    }

    public StageGirl GetFromDressId(string other)
    {
        return DressDict[other];
    }

    public string GetEmoteFromIcon(string name)
    {
        return IconsDict.ContainsKey(name) ? IconsDict[name] : "<:please_ping_if_you_see_this:670781960800698378>";
    }
    
    private static List<T> DeserializeJson<T>(string path)
    {
        var f = File.ReadAllText(path);
        var jsonList = JsonConvert.DeserializeObject<List<T>>(f);

        if (jsonList is not null) return jsonList;
        Console.WriteLine($"Failed to open file '{path}'"); 
        return new List<T>();
    }

    public async Task<IEnumerable<AutocompleteResult>> AutoCompleteFilter(string other)
    {
        await Program.LogAsync(new LogMessage(LogSeverity.Debug, "autocomp", "Generating autocomplete results..."));
        
        var a = DressDict.Keys.Where(s => s.StartsWith(other)).Take(10).ToList();

        return a.Select(c => new AutocompleteResult(DressDict[c].Name, c)).ToList();
    }
}