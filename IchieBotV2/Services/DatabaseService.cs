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
    private List<StageGirl> DressList { get; set; }
    private Dictionary<string,StageGirl> DressDict { get; set; }
    
    private Dictionary<string,string> IconsDict { get; set; }

    public DatabaseService(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
    {
        _client = client;
        _commands = commands;
        _services = services;
        DressDict = new Dictionary<string, StageGirl>();

        DressList = DeserializeJson<StageGirl>(@"Legacy/json/stagegirls.json");
        foreach(var d in DressList)
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

    public IEnumerable<StageGirl> LegacySearch(string keyword)
    {
        var cleanedInput = CleanString(keyword);

        var keywordsSplit = keyword.Split(' ').ToList().Select(CleanString).ToList();

        var perfectMatch = DressList.Where(s => CleanString(s.Name) == cleanedInput).ToList();
        if (perfectMatch.Count > 0)
            return perfectMatch.ToList();

        var results = DressList.Where(s => CleanString(s.Name).Contains(cleanedInput) ||
                                           keywordsSplit.All(s.Name.ToLowerInvariant().Contains) ||
                                           s.Aliases.Any(alias => keywordsSplit.All(alias.ToLowerInvariant().Contains))).ToList();

        if (results.Count == 0)
        {
            results = DressList.Where(s => keywordsSplit.All(key => s.Name.Split().Select(CleanString)
                                               .Any(str => GetDistance(str, key) < 2)) || 
                                           s.Aliases.Any(alias => GetDistance(CleanString(alias), cleanedInput) < 2)).ToList();
        }

        return results;
    }

    public static string CleanString(string s)
    {
        var result = s.ToLowerInvariant();
        result = string.Join("", result.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        var arr = result.ToCharArray();
        arr = Array.FindAll(arr, char.IsLetter);
        return new string(arr);
    }
    
    /// <summary>
    /// Calculates the Damerau-Levenshtein distance between two strings.
    /// This distance is determined by how many operations (Insertions, deletions, substitutions) it'd take
    /// to get from one of the string to the other.
    /// </summary>
    /// <returns>Damerau-Levenshtein distance (How close the strings look like)</returns>
    public static int GetDistance(string s1, string s2)
    {
        var bounds = new {Height = s1.Length + 1, Width = s2.Length + 1};
        var matrix = new int[bounds.Height, bounds.Width];
        for (var height = 0; height < bounds.Height; height++)
        {
            matrix[height, 0] = height;
        }
        for (var width = 0; width < bounds.Width; width++)
        {
            matrix[0, width] = width;
        }

        for (var height = 1; height < bounds.Height; height++)
        {
            for (var width = 1; width < bounds.Width; width++)
            {
                var cost = (s1[height - 1] == s2[width - 1]) ? 0 : 1;
                var insert = matrix[height, width - 1] + 1;
                var delete = matrix[height - 1, width] + 1;
                var substitute = matrix[height - 1, width - 1] + cost;
					
                var distance = Math.Min(Math.Min(delete, substitute), insert);

                if (height > 1 && width > 1 && s1[height - 1] == s2[width - 2] && s1[height - 2] == s2[width - 1])
                {
                    distance = Math.Min(distance, matrix[height - 2, width - 2] + cost);
                }

                matrix[height, width] = distance;
            }
        }

        return matrix[bounds.Height - 1, bounds.Width - 1];
    }

    public async Task<IEnumerable<AutocompleteResult>> AutoCompleteFilter(string other)
    {
        await Program.LogAsync(new LogMessage(LogSeverity.Debug, "autocomp", "Generating autocomplete results..."));

        var search = LegacySearch(other).Take(25);
        return search.Select(s => new AutocompleteResult(s.Name, s.DressId[2..]));
    }
}