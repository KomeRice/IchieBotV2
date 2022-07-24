using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using IchieBotData.Common;
using IchieBotData.Legacy;
using IchieBotV2.Utils;
using Newtonsoft.Json;
using Icon = IchieBotData.Common.Icon;

namespace IchieBotV2.Services;

public class DatabaseService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    
    public List<StageGirl> DressListLegacy { get; set; }
    private Dictionary<string,StageGirl> DressDict { get; set; }

    public StatCalculator Calculator { get; set; }
    
    private Dictionary<string,string> IconsDict { get; set; }
    
    // TODO: Error checking
    public const string CachePath = @"Data/Cache/";
    public Dictionary<string, List<List<int>>>? RbCache;

    public readonly Dictionary<string, List<string>> SearchCache;
    private const int MaxSearchCacheSize = 64;

    // Temporary plug until Karthuria updates its format
    public Dictionary<string, ComplementJson> DictComplements = new();

    public DatabaseService(DiscordSocketClient client, InteractionService commands, IServiceProvider services, StatCalculator calculator)
    {
        _client = client;
        _commands = commands;
        _services = services;
        Calculator = calculator;
        DressListLegacy = new List<StageGirl>();
        IconsDict = new Dictionary<string, string>();
        DressDict = new Dictionary<string, StageGirl>();
        SearchCache = new Dictionary<string, List<string>>(64);

        Refresh();
    }

    public void Refresh()
    {
        LoadJson();
        LoadReproductionCache();
        ResetSearchCache();
    }

    public void CacheValue(string key, List<string> values)
    {
        if (SearchCache.Keys.Count > MaxSearchCacheSize)
        {
            SearchCache.Remove(SearchCache.Keys.First());
        }

        SearchCache[key] = values;
    }

    public void ResetSearchCache()
    {
        SearchCache.Clear();
    }

    public void LoadJson()
    {
        DressDict = new Dictionary<string, StageGirl>();
        DressListLegacy = DeserializeJson<StageGirl>(@"Legacy/json/stagegirls.json");
        DressListLegacy.Sort();
        foreach(var d in DressListLegacy)
        {
            DressDict.Add(d.DressId[2..], d);
        }

        IconsDict = new Dictionary<string, string>();
        
        foreach (var i in DeserializeJson<Icon>(@"Data/Icons.json"))
        {
            IconsDict[i.Name] = i.Emote;
        }
        
        DictComplements = JsonConvert.DeserializeObject<Dictionary<string, ComplementJson>>(File.ReadAllText(@"Data/Complement.json")) 
                          ?? throw new InvalidOperationException();
    }

    public StageGirl GetFromDressId(string other)
    {
        return DressDict[other];
    }

    public string GetEmoteFromIcon(string name)
    {
        return IconsDict.ContainsKey(name) ? IconsDict[name] : "<:please_ping_if_you_see_this:670781960800698378>";
    }

    public List<int> GetDressStats(string dressId, int rb = 0)
    {
        if (rb != 0 && !Calculator.HasRemake(dressId))
            rb = 0;

        var parameters = Calculator.GetDressStats(dressId, level: 80 + 5 * rb, remake: rb, friendship: 30 + 5 * rb);
        return parameters.ToIntList();
    }

    public List<List<int>> GetAllRbStats(string dressId)
    {
        if (!Calculator.HasRemake(dressId))
            throw new ArgumentException("Passed dressId does not have Reproduction stats");

        var ret = new List<List<int>>();
        for (var i = 1; i < 5; i++)
        {
            ret.Add(GetDressStats(dressId, i));
        }

        return ret;
    }

    public async Task<List<int>?> GetFromReproductionCache(string dressId, int rb)
    {
        if (!Calculator.HasRemake(dressId))
            return null;
        if (rb is < 1 or > 4)
            throw new ArgumentException($"Rb level must be between 1 and 4 (Got {rb})");

        if (RbCache is not null && RbCache.ContainsKey(dressId)) 
            return RbCache[dressId][rb - 1];
        try
        {
            await BuildReproductionCache();
            if (!RbCache!.ContainsKey(dressId))
                throw new Exception($"No entry for {dressId}");
        }
        catch (Exception e)
        {
            await Program.LogAsync(new LogMessage(LogSeverity.Critical, "dbCache",
                $"Failed to build cache for {dressId}, falling back to recalculating.", e));
            return GetDressStats(dressId, rb);
        }

        return RbCache[dressId][rb - 1];
    }

    public void LoadReproductionCache(string path = CachePath + "rbCache.json")
    {
        if (!File.Exists(path))
            Task.Run(BuildReproductionCache).Wait();
        var jsonCache = File.ReadAllText(path);
        var cache = JsonConvert.DeserializeObject<Dictionary<string,List<List<int>>>>(jsonCache);
        RbCache = cache;
    }

    public async Task BuildReproductionCache()
    {
        const string path = CachePath + "rbCache.json";

        var cache = new Dictionary<string, List<List<int>>>();
        if (cache == null) 
            throw new ArgumentNullException(nameof(cache));

        foreach (var dressId in DressDict.Keys.Where(Calculator.HasRemake))
        {
            cache[dressId] = GetAllRbStats(dressId);
        }
        var jsonCache = JsonConvert.SerializeObject(cache);
        Directory.CreateDirectory(CachePath);
        await File.WriteAllTextAsync(path, jsonCache);
        RbCache = cache;
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
        if (SearchCache.ContainsKey(cleanedInput))
            return SearchCache[cleanedInput].Select(id => DressDict[id]);

        var keywordsSplit = keyword.Split(' ').ToList().Select(CleanString).ToList();

        var perfectMatch = DressListLegacy.Where(s => CleanString(s.Name) == cleanedInput).ToList();
        if (perfectMatch.Count > 0)
            return perfectMatch.ToList();

        var results = DressListLegacy.Where(s => CleanString(s.Name).Contains(cleanedInput) ||
                                           keywordsSplit.All(s.Name.ToLowerInvariant().Contains) ||
                                           s.Aliases.Any(alias => keywordsSplit.All(alias.ToLowerInvariant().Contains))).ToList();

        if (results.Count == 0)
        {
            results = DressListLegacy.Where(s => keywordsSplit.All(key => s.Name.Split().Select(CleanString)
                                               .Any(str => GetDistance(str, key) < 2)) || 
                                           s.Aliases.Any(alias => GetDistance(CleanString(alias), cleanedInput) < 2)).ToList();
        }

        return results;
    }
    
    public List<StageGirl> TrySearch(string uniqueId)
    {
        var split = uniqueId.Split("_");
        var name = split[0];
        Element? element = split[1] == "x" ? null : (Element) Convert.ToInt32(split[1]);
        Row? row = split[2] == "x" ? null : (Row) Convert.ToInt32(split[2]);
        Pool? pool = split[3] == "x" ? null : (Pool) Convert.ToInt32(split[3]);
        Cost? cost = split[4] == "x" ? null : (Cost) Convert.ToInt32(split[4]);
        Rarity? rarity = split[5] == "x" ? null : (Rarity) Convert.ToInt32(split[5]);
        AttackType? type = split[6] == "x" ? null : (AttackType) Convert.ToInt32(split[6]) ;
        School? school = split[7] == "x" ? null : (School) Convert.ToInt32(split[7]);

        if (SearchCache.ContainsKey(uniqueId))
        {
            return SearchCache[uniqueId].Select(GetFromDressId).ToList();
        }
        
        var dressList = LegacySearch(name).Where(d =>
            (element is null || LegacyNameToElement(d.Element.Name) == element) &&
            (row is null || d.Row.Name == row.ToString()?.ToLowerInvariant()) &&
            (pool is null || d.Pool == pool.ToString()) &&
            (cost is null || $"Cost{DictComplements[d.DressId[2..]].Cost}" == cost.ToString()) &&
            (rarity is null || $"Star{d.Rarity}" == rarity.ToString()) &&
            (type is null || !(type == AttackType.Special ^ d.Special)) &&
            BelongsToSchool(d, school)).OrderByDescending(s => s.Rarity).ThenBy(s => s.Name).ToList();
        
        if(dressList.Count > 1)
            CacheValue(uniqueId, dressList.Select(s => s.DressId[2..]).ToList());

        return dressList;
    }

    public static string GetUniqueId(string name = "",
        Element? element = null,
        Row? row = null,
        Pool? pool = null,
        Cost? cost = null,
        Rarity? rarity = null,
        AttackType? type = null,
        School? school = null)
    {
        var args = string.Join("_", new List<string>
        {
            element is null ? "x" : ((int)element).ToString(),
            row is null ? "x" : ((int)row).ToString(),
            pool is null ? "x" : ((int) pool).ToString(),
            cost is null ? "x" : ((int) cost).ToString(),
            rarity is null ? "x" : ((int) rarity).ToString(),
            type is null ? "x" : ((int) type).ToString(),
            school is null ? "x" : ((int) school).ToString()
        });

        return $"{CleanString(name)}_{args}";
    }

    private static Element LegacyNameToElement(string name)
    {
        return name switch
        {
            "cosmos" => Element.Space,
            "cloud" => Element.Cloud,
            "moon" => Element.Moon,
            "flower" => Element.Flower,
            "snow" => Element.Snow,
            "wind" => Element.Wind,
            "dream" => Element.Dream,
            _ => Element.NoElem
        };
    }

    private static bool BelongsToSchool(StageGirl d, School? s)
    {
        return s is null || d.DressId[2..].StartsWith(((int) s).ToString());
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

    public IEnumerable<AutocompleteResult> AutoCompleteFilter(string query)
    {
        var search = LegacySearch(query).Take(25);
        return search.Select(s => new AutocompleteResult(s.Name, s.DressId[2..]));
    }

    public class ComplementJson
    {
        public int Cost { get; set; }
        public int RowIndex { get; set; }
        public Dictionary<string, int?> Release = new();
    }
}