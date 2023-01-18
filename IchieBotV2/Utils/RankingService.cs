using IchieBotData;
using IchieBotV2.Services;

namespace IchieBotV2.Utils;

public class RankingService
{
    private readonly DatabaseService _db;
    private readonly List<List<int>> _dressRanking = new List<List<int>>();
    private readonly List<List<List<int>>> _rbDressRanking = new List<List<List<int>>>();
    
    private readonly Dictionary<string, List<int>> _rankDict = new Dictionary<string, List<int>>();
    private readonly List<List<SortedDictionary<int,string>>> _reverseRanks = new List<List<SortedDictionary<int, string>>>();
    private readonly Dictionary<string, int> _rowIndex = new Dictionary<string, int>();
    private readonly SortedDictionary<int, string> _reverseRowIndex = new SortedDictionary<int, string>();

    public RankingService(DatabaseService db)
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

    private async Task InitializeRanking()
    {
        var dresses = _db.Dresses;

        foreach (var dress in dresses.Keys.Select(dressKey => dresses[dressKey]))
        {
            for (var i = 0; i < dress.Stats[0].Count; i++)
            {
                _dressRanking[i].Add(dress.Stats[0][i]);
                if (dress.HasRemake)
                {
                    for(var j = 0; j < 4; j++)
                    {
                        _rbDressRanking[i][j].Add(dress.Stats[j + 1][i]);
                    }
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

        // Iteration over dresses could be reduced
        foreach (var dress in dresses.Keys.Select(dressKey => dresses[dressKey]))
        {
            var ranks = new List<int>();

            for (var i = 0; i < dress.Stats[0].Count; i++)
            {
                var rank = _dressRanking[i].IndexOf(dress.Stats[0][i]) + 1;
                ranks.Add(rank);
                if (_reverseRanks[i][0].ContainsKey(rank))
                {
                    _reverseRanks[i][0][rank] += $",{dress.DressId}";
                }
                else
                {
                    _reverseRanks[i][0][rank] = dress.DressId;
                }
            }

            _rankDict[dress.DressId] = ranks;
            
            if(!dress.HasRemake)
                continue;

            for (var i = 1; i < 5; i++)
            {
                var rbRanks = new List<int>();
                var stat = dress.Stats[i];

                for (var j = 0; j < dress.Stats[0].Count; j++)
                {
                    var rank = _rbDressRanking[j][i - 1].IndexOf(stat[j]) + 1;
                    rbRanks.Add(rank);
                    if (_reverseRanks[j][i].ContainsKey(rank))
                    {
                        _reverseRanks[j][i][rank] += $",{dress.DressId}";
                    }
                    else
                    {
                        _reverseRanks[j][i][rank] = dress.DressId;
                    }
                }

                _rankDict[dress.DressId + $"_rb{i}"] = rbRanks;
            }
        }

        var rowIndexSort = dresses.Values
            .OrderBy(d => d.RowIndex)
            .ThenByDescending(d => d.DressId)
            .ToList();

        for (var i = 0; i < rowIndexSort.Count; i++)
        {
            _rowIndex[rowIndexSort[i].DressId] = i + 1;
            _reverseRowIndex[i + 1] = rowIndexSort[i].DressId;
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