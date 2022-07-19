using Newtonsoft.Json.Linq;

namespace IchieBotV2.Utils;

public class StatCalculator
{
    private Dictionary<string, JObject> RawData { get; set; }

    public class Parameter
    {
        // ReSharper disable class InconsistentNaming
        public const string HP = "hp";
        public const string ATK = "atk";
        public const string PDEF = "pdef";
        public const string MDEF = "mdef";
        public const string AGI = "agi";

        public static List<string> GetParameterNames()
        {
            return new List<string>() {HP, ATK, PDEF, MDEF, AGI};
        }
    }

    public class Parameters : Dictionary<string, int>
    {
        public static Parameters operator +(Parameters a, Parameters b)
        {
            var ret = new Parameters();
            foreach (var p in a.Keys)
            {
                ret[p] = a[p] + b[p];
            }
            return ret;
        }
        public static Parameters operator -(Parameters a, Parameters b)
        {
            var ret = new Parameters();
            foreach (var p in a.Keys)
            {
                ret[p] = a[p] - b[p];
            }
            return ret;
        }

        public static Parameters operator *(Parameters a, int b)
        {
            var ret = new Parameters();
            foreach (var p in a.Keys)
            {
                ret[p] = a[p] * b;
            }
            return ret;
        }

        public static Parameters operator /(Parameters a, int b)
        {
            var ret = new Parameters();
            foreach (var p in a.Keys)
            {
                ret[p] = (int) Math.Floor((double) a[p] / b);
            }
            return ret;
        }

        public override string ToString()
        {
            return string.Join(", ", Keys.Select(p => $"{p}: {this[p]}").ToList());
        }

        public List<int> ToIntList()
        {
            return Parameter.GetParameterNames().Select(p => this[p]).ToList();
        }
    }

    private readonly Dictionary<int, string> _growthBoardTypes = new Dictionary<int, string>()
    {
        {11, Parameter.HP},
        {12, Parameter.ATK},
        {13, Parameter.PDEF},
        {14, Parameter.MDEF},
        {15, Parameter.AGI}
    };
    private readonly Dictionary<int, int> _rarityGrowthRates = new Dictionary<int, int>()
    {
        {1, 0},
        {2, 0},
        {3, 10},
        {4, 25},
        {5, 50},
        {6, 75},
    };
    private readonly Dictionary<int, int> _boardRankGrowthRates = new Dictionary<int, int>()
    {
        {1, 0},
        {2, 15},
        {3, 30},
        {4, 45},
        {5, 60},
        {6, 80},
        {7, 100},
        {8, 110},
        {9, 120},
    };
    private readonly bool[] defaultBordPanelMask = new bool[] {true, true, true, true, true, true, true, true};

    public StatCalculator()
    {
        RawData = new Dictionary<string, JObject>();

        foreach (var filename in Directory.GetFiles(@"Data/Raw"))
        {
            var f = File.ReadAllText(filename);
            var separators = new char[]{'/', '\\'};
            RawData[filename.Split(separators).Last()] = JObject.Parse(f);
        }
    }

    public Parameters GetDressStats(string dressId, int rarity = 6, int level = 80, int friendship = 30, int boardRank = 9,
        bool[]? boardPanelMask = null, int remake = 0)
    {
        try
        {
            boardPanelMask ??= defaultBordPanelMask;
            var dress = RawData["dress.json"][dressId];
            var dressRemakeParameter = RawData["dress_remake_parameter.json"];
            var growthBoard = RawData["growth_board.json"];
            var growthPanel = RawData["growth_panel.json"];
            var friendshipPatterns = RawData["friendship_pattern.json"];
            
            if (dress is null || dressRemakeParameter is null || growthBoard is null ||
                growthPanel is null || friendshipPatterns is null)
            {
                throw new NullReferenceException("Failed to load dress object from raw data");
            }

            var baseParameters = new Parameters();
            var deltaParameters = new Parameters();

            foreach (var p in Parameter.GetParameterNames())
            {
                baseParameters[p] = Convert.ToInt32(dress[$"base_{p}"]?.ToString());
                deltaParameters[p] = Convert.ToInt32(dress[$"delta_{p}"]?.ToString());
            }

            var referenceParameters = (baseParameters + deltaParameters * (80 + 5 * remake - 1) / 1000)
                * (100 + _boardRankGrowthRates[9] + _rarityGrowthRates[6]) / 100;

            var friendshipPattern =
                friendshipPatterns[dress["friendship_pattern_id"]?.ToString() ?? throw new InvalidOperationException()];
            var friendshipPanels = (friendshipPattern ?? throw new InvalidOperationException()).Take(friendship);

            var growthBoards = new List<JToken?>();
            for (var i = 1; i < 10; i++)
            {
                growthBoards.Add(growthBoard[dress[$"growth_board{i}_id"]!.ToString() ?? throw new InvalidOperationException()]);
            }

            growthBoards = growthBoards.Take(boardRank).ToList();

            var growthBoardPanels = new List<List<JToken?>>();
            foreach (var board in growthBoards)
            {
                var gPanels = new List<JToken?>();
                for(var i = 1; i < 9; i++)
                    gPanels.Add(growthPanel[board![$"panel{i}_id"]!.ToString() ?? throw new InvalidOperationException()]);
                growthBoardPanels.Add(gPanels);
            }

            var panels = new List<Tuple<string, JToken?>>();
            foreach (var friendshipPanel in friendshipPanels)
            {
                var fPanel =((JProperty) friendshipPanel).Value;
                var effectType = Convert.ToInt32(fPanel["effect_type"]!.ToString());
                if (_growthBoardTypes.ContainsKey(effectType))
                {
                    panels.Add(new Tuple<string, JToken?>
                        (_growthBoardTypes[Convert.ToInt32(effectType.ToString())], fPanel["effect_arg1"]));
                }
            }

            foreach(var gBoard in growthBoardPanels.SkipLast(1))
            {
                foreach (var gPanel in gBoard)
                {
                    var effectType = Convert.ToInt32(gPanel!["type"]!.ToString());

                    if (_growthBoardTypes.ContainsKey(effectType))
                    {
                        panels.Add(new Tuple<string, JToken?>(_growthBoardTypes[effectType], gPanel["value"]));
                    }
                }
            }

            foreach (var (enable, gPanel) in boardPanelMask.Zip(growthBoardPanels.Last()))
            {
                if (!enable) continue;
                var effectType = Convert.ToInt32(gPanel!["type"]!.ToString());
                if (_growthBoardTypes.ContainsKey(effectType))
                {
                    panels.Add(new Tuple<string, JToken?>(_growthBoardTypes[effectType], gPanel["value"]));
                }
            }

            var panelParameters = new Parameters();
            foreach (var p in Parameter.GetParameterNames())
                panelParameters[p] = 0;

            foreach (var (panelType, panelValue) in panels)
            {
                panelParameters[panelType] += (int) Math.Floor(Convert.ToDouble(
                    panelValue!.ToString()) * referenceParameters[panelType] / 100);
            }

            var levelParams = (baseParameters + deltaParameters * (level - 1) / 1000);
            levelParams *= 100 + _boardRankGrowthRates[boardRank] + _rarityGrowthRates[rarity];
            levelParams /= 100;

            if (remake > 0)
            {
                var remakeParameters = dressRemakeParameter[dressId]![remake.ToString()];
                var remakeParametersFinal = new Parameters();
                foreach (var p in Parameter.GetParameterNames())
                {
                    remakeParametersFinal[p] = Convert.ToInt32(remakeParameters![p]!.ToString());
                }

                return levelParams + panelParameters + remakeParametersFinal;
            }

            return levelParams + panelParameters;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null!;
        }
    }
    
    public bool HasRemake(string dressId)
    {
        return RawData["dress_remake_parameter.json"].ContainsKey(dressId);
    } 
}