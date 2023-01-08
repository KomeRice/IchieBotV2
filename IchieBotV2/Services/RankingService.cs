namespace IchieBotV2.Services;

public class RankingService
{
    private readonly DatabaseLegacyService _db;
    private readonly List<List<int>> _dressRanking = new List<List<int>>();
    private readonly List<List<List<int>>> _rbDressRanking = new List<List<List<int>>>();
    
    private readonly Dictionary<string, List<int>> _rankDict = new Dictionary<string, List<int>>();
    private readonly List<List<SortedDictionary<int,string>>> _reverseRanks = new List<List<SortedDictionary<int, string>>>();
    private readonly Dictionary<string, int> _rowIndex = new Dictionary<string, int>();
    private readonly SortedDictionary<int, string> _reverseRowIndex = new SortedDictionary<int, string>();

    public RankingService(DatabaseLegacyService db)
    {
        _db = db;
        for (var i = 0; i < 6; i++)
        {
            _dressRanking.Add(new List<int>());
            _rbDressRanking.Add(new List<List<int>>());
            _reverseRanks.Add(new List<SortedDictionary<int, string>>());
            _reverseRanks[i].Add(new SortedDictionary<int, string>());
            for (var j = 0; j < 4; j++)
            {
                _rbDressRanking[i].Add(new List<int>());
                _reverseRanks[i].Add(new SortedDictionary<int, string>());
            }
        }
        
        Task.Run(InitializeRanking).Wait();
    }

    public async Task InitializeRanking()
    {
        var dressList = _db.DressListLegacy;
        
        foreach (var d in dressList)
        {
            List<List<int>?>? rbStats = null;
            var dressId = d.DressId[2..];
            var hasRemake = _db.Calculator.HasRemake(dressId);
            if (hasRemake)
            {
                rbStats = new List<List<int>?>();
                for (var j = 1; j < 5; j++)
                {
                    rbStats.Add(await _db.GetFromReproductionCache(dressId, j));
                }
            }
            
            for (var i = 0; i < d.MaxStats.Count; i++)
            {
                _dressRanking[i].Add(d.MaxStats[i]);
                if (!hasRemake) continue;
                for (var j = 0; j < rbStats!.Count; j++)
                {
                    _rbDressRanking[i][j].Add(rbStats[j]![i]);
                }
            }
        }

        foreach (var list in _dressRanking)
        {
            list.Sort((a, b) => b.CompareTo(a));
        }

        foreach (var list in _rbDressRanking.SelectMany(rbList => rbList))
        {
            list.Sort((a, b) => b.CompareTo(a));
        }

        foreach (var d in dressList)
        {
            var dressId = d.DressId[2..];
            var ranks = new List<int>();

            for (var i = 0; i < d.MaxStats.Count; i++)
            {
                var rank = _dressRanking[i].IndexOf(d.MaxStats[i]) + 1;
                ranks.Add(rank);
                if (_reverseRanks[i][0].ContainsKey(rank))
                {
                    _reverseRanks[i][0][rank] += $",{dressId}";
                }
                else
                {
                    _reverseRanks[i][0][rank] = dressId;
                }
            }

            _rankDict[dressId] = ranks;
            
            if (_db.Calculator.HasRemake(dressId))
            {
                for (var i = 1; i < 5; i++)
                {
                    var rbRanks = new List<int>();
                    var stat = await _db.GetFromReproductionCache(dressId, i);
                    
                    if (stat == null)
                        throw new NullReferenceException("Failed RB array stat access");
                    for (var j = 0; j < stat.Count; j++)
                    {
                        var rank = _rbDressRanking[j][i - 1].IndexOf(stat[j]) + 1;
                        rbRanks.Add(rank);
                        if (_reverseRanks[j][i].ContainsKey(rank))
                        {
                            _reverseRanks[j][i][rank] += $",{dressId}";
                        }
                        else
                        {
                            _reverseRanks[j][i][rank] = dressId;
 
                        }
                    }
                    _rankDict[dressId + $"_rb{i}"] = rbRanks;
                }
            }
        }

        var rowIndexSort = _db.DictComplements
            .OrderBy(c => c.Value.RowIndex)
            .ThenByDescending(c => c.Key).ToList();

        for (var i = 0; i < rowIndexSort.Count; i++)
        {
            _rowIndex[rowIndexSort[i].Key] = i + 1;
            _reverseRowIndex[i + 1] = rowIndexSort[i].Key;
        }
    }

    public List<int> GetRanks(string dressId, int rb = 0)
    {
        return _rankDict[dressId + (rb != 0 ? $"_rb{rb}" : "")];
    }

    public int GetPositionIndex(string dressId)
    {
        return _rowIndex[dressId];
    }

    public SortedDictionary<int, string> GetRanking(Parameter p, int rb = 0)
    {
        return p == Parameter.RowPosition ? _reverseRowIndex : _reverseRanks[(int) p][rb];
    }

    public int GetMax(Parameter p, int rb = 0)
    {
        if (p == Parameter.RowPosition)
            return _rowIndex.Count;
        return rb == 0 ? _dressRanking[(int)p].Count : _rbDressRanking[(int)p][rb - 1].Count;
    }
    
    public enum Parameter
    {
        PowerScore = 0,
        MaxHp = 1,
        Act = 2,
        NormDef = 3,
        SpDef = 4,
        Agility = 5,
        RowPosition = 6
    }
}