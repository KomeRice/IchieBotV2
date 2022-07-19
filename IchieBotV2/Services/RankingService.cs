namespace IchieBotV2.Services;

public class RankingService
{
    private readonly DatabaseService _db;
    private List<List<int>> DressRanking = new List<List<int>>();
    private List<List<List<int>>> RbDressRanking = new List<List<List<int>>>();
    
    private Dictionary<string, List<int>> RankDict = new Dictionary<string, List<int>>();
 

    public RankingService(DatabaseService db)
    {
        _db = db;
        for (var i = 0; i < 6; i++)
        {
            DressRanking.Add(new List<int>());
            RbDressRanking.Add(new List<List<int>>());
            for(var j = 0; j < 4; j++)
                RbDressRanking[i].Add(new List<int>());
        }
        
        Task.Run(InitializeRanking).Wait();
    }

    public async Task InitializeRanking()
    {
        var dressList = _db.DressListLegacy;
        
        foreach (var d in dressList)
        {
            List<List<int>?>? rbStats = null;
            var hasRemake = _db.Calculator.HasRemake(d.DressId[2..]);
            if (hasRemake)
            {
                rbStats = new List<List<int>?>();
                for (var j = 1; j < 5; j++)
                {
                    rbStats.Add(await _db.GetFromCache(d.DressId[2..], j));
                }
            }
            
            for (var i = 0; i < d.MaxStats.Count; i++)
            {
                DressRanking[i].Add(d.MaxStats[i]);
                if (!hasRemake) continue;
                for (var j = 0; j < rbStats!.Count; j++)
                {
                    RbDressRanking[i][j].Add(rbStats[j]![i]);
                }
            }
        }

        foreach (var list in DressRanking)
        {
            list.Sort((a, b) => b.CompareTo(a));
        }

        foreach (var list in RbDressRanking.SelectMany(rbList => rbList))
        {
            list.Sort((a, b) => b.CompareTo(a));
        }

        foreach (var d in dressList)
        {
            var dressId = d.DressId[2..];
            var ranks = new List<int>();

            for (var i = 0; i < d.MaxStats.Count; i++)
            {
                ranks.Add(DressRanking[i].IndexOf(d.MaxStats[i]));
            }

            RankDict[dressId] = ranks;
            
            if (_db.Calculator.HasRemake(d.DressId[2..]))
            {
                for (var i = 1; i < 5; i++)
                {
                    var rbRanks = new List<int>();
                    var stat = await _db.GetFromCache(dressId, i);
                    
                    if (stat == null)
                        throw new NullReferenceException("Failed RB array stat access");
                    for (var j = 0; j < stat.Count; j++)
                    {
                        rbRanks.Add(RbDressRanking[j][i - 1].IndexOf(stat[j]));
                    }
                    RankDict[dressId + $"_rb{i}"] = rbRanks;
                }
            }
        }

        await Task.Delay(1);
    }
}