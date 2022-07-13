using System.Collections;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using OldJsonFormatParser.LegacyClass;

namespace IchieBotV2.Services;

public class DatabaseService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private List<StageGirl> Dress { get; set; }
    private Dictionary<string,StageGirl> Dict { get; set; }

    public DatabaseService(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
    {
        _client = client;
        _commands = commands;
        _services = services;
        Dict = new Dictionary<string, StageGirl>();

        Dress = DeserializeJson(@"Legacy/json/stagegirls.json");
        foreach(var d in Dress)
        {
            Dict.Add(d.DressId[2..], d);
        }
    }

    public StageGirl GetFromDressId(string other)
    {
        return Dict[other];
    }
    
    private List<StageGirl> DeserializeJson(string path)
    {
        var f = File.ReadAllText(path);
        var dressList = JsonConvert.DeserializeObject<List<StageGirl>>(f);

        if (dressList is not null) return dressList;
        Console.WriteLine($"Failed to open file '{path}'"); 
        return new List<StageGirl>();
    }

    public async Task<IEnumerable<AutocompleteResult>> AutoCompleteFilter(string other)
    {
        await Program.LogAsync(new LogMessage(LogSeverity.Debug, "autocomp", "Generating autocomplete results..."));
        
        var a = Dict.Keys.Where(s => s.StartsWith(other)).Take(10).ToList();

        return a.Select(c => new AutocompleteResult(Dict[c].Name, c)).ToList();
    }
}