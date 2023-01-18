using Discord;
using IchieBotData;
using IchieBotV2.Services;

namespace IchieBotV2.Utils;

public class DressEmbedHelper
{
    private readonly DatabaseService _db;
    private readonly RankingService _ranking;

    private static readonly List<string> MenuEntries = new List<string>() {"Overview", "Skills", "Misc."};
    private const int MaxPageSize = 14;
    
    private List<double> _relevantSpeeds = new List<double>()
    {
        1.42, 1.22, 1.19, 1.15, 1.12, 1.10, 1.09, 1.08, 1.0, 0.85, 0.82, 0.7
    };

    public DressEmbedHelper(DatabaseService db, RankingService rankingService)
    {
        _db = db;
        _ranking = rankingService;
    }

    public async Task<List<ActionRowBuilder>> DressEmbedMenu(string uniqueId)
    {
        DressMenuId dressMenuData;
        try
        {
            dressMenuData = new DressMenuId(uniqueId);
        }
        catch (ArgumentException e)
        {
            await Program.LogAsync(new LogMessage(LogSeverity.Error, "dressEmbedHelper",
                "Wrong Unique id given to dress embed helper"));
            return new List<ActionRowBuilder>();
        }

        var disabledIndex = 0;
        var rows = new List<ActionRowBuilder>();
        if (_db.Calculator.HasRemake(dressMenuData.DressId))
        {
            var rbRow = new ActionRowBuilder();
            for (var rb = 0; rb < 5; rb++)
            {
                var isCurRb = rb == dressMenuData.Remake;
                if (isCurRb)
                {
                    rbRow.WithButton($"RB{rb}",
                        $"disabled_{disabledIndex++}",
                        ButtonStyle.Success, disabled: true);
                    continue;
                }
                rbRow.WithButton($"RB{rb}", 
                    dressMenuData.ToCustomString(rb));
            }
            rows.Add(rbRow);
        }

        var menuRow = new ActionRowBuilder();

        for (var i = 0; i < MenuEntries.Count; i++)
        {
            var isCurMenu = i == dressMenuData.MenuId;
            if (isCurMenu)
            {
                menuRow.WithButton(MenuEntries[i],
                    $"disabled_{disabledIndex++}",
                    ButtonStyle.Success, disabled: true);
                continue;
            }

            menuRow.WithButton(MenuEntries[i],
                dressMenuData.ToCustomString(menuId: i));
        }
        rows.Add(menuRow);

        return rows;
    }

    public Embed DressEmbedOverview(Dress dress, int rb = 0)
    {
        var author = new EmbedAuthorBuilder()
        {
            IconUrl = dress.ThumbUrl,
            Name = $"{dress.BaseRarity}★ {dress.Name} [{dress.Pool}]"
        };

        var description = $"{dress.Element} | {dress.Type}" +
                          $"\nCost: {dress.Cost}" +
                          $"\nReleased (JP): <t:{dress.ReleaseJP}>";
        description += dress.ReleaseWW == 0
            ? $"\nReleased (WW): <t:{dress.ReleaseWW}>"
            : "\nUnreleased (WW)";

        var stats = dress.Stats[rb];
        var ranks = _ranking.GetRanks(dress.DressId, rb);
        
        var embedFieldBuilders = new List<EmbedFieldBuilder>()
        {
            new EmbedFieldBuilder()
            {
                Name = "Maximal Stats",
                Value = "```" + "Power Score".PadRight(18) + $"{stats[0]}".PadRight(10) +
                        $"[#{ranks[0]}/{_ranking.GetMax(RankingService.Parameter.PowerScore, rb)}]\n" +
                        "MaxHP".PadRight(18) + $"{stats[1]}".PadRight(10) +
                        $"[#{ranks[1]}/{_ranking.GetMax(RankingService.Parameter.MaxHp, rb)}]\n" +
                        "ACT Power".PadRight(18) + $"{stats[2]}".PadRight(10) +
                        $"[#{ranks[2]}/{_ranking.GetMax(RankingService.Parameter.Act, rb)}]\n" +
                        "NormDef".PadRight(18) + $"{stats[3]}".PadRight(10) +
                        $"[#{ranks[3]}/{_ranking.GetMax(RankingService.Parameter.NormDef, rb)}]\n" +
                        "SpDef".PadRight(18) + $"{stats[4]}".PadRight(10) +
                        $"[#{ranks[4]}/{_ranking.GetMax(RankingService.Parameter.SpDef, rb)}]\n" +
                        "Agility".PadRight(18) + $"{stats[5]}".PadRight(10) +
                        $"[#{ranks[5]}/{_ranking.GetMax(RankingService.Parameter.Agility, rb)}]\n" +
                        "Row Position".PadRight(18) + _ranking.GetPositionIndex(dress.DressId) + "\n" +
                        "Ranked Frontmost to Backmost```"
            }
        };

        embedFieldBuilders.AddRange( dress.BasicActs.Select((curAct, i) => GetActEmbed(curAct, i + 1)));
        embedFieldBuilders.Add(GetActEmbed(dress.ClimaxAct, 1, true));

        var e = new EmbedBuilder()
        {
            Author = author,
            Description = description,
            Fields = embedFieldBuilders,
            ThumbnailUrl = dress.ThumbUrl
        };
        
        return e.Build();
    }

    public Embed DressEmbedSkills(Dress dress)
    {
        var author = new EmbedAuthorBuilder()
        {
            IconUrl = dress.ThumbUrl,
            Name = $"{dress.BaseRarity}★ {dress.Name} [{dress.Pool}]"
        };

        var description = dress.Aliases.Count > 0
            ? $"Aliases: *{string.Join(", ", dress.Aliases)}*"
            : "*No aliases*";

        var fields = dress.AutoSkills.Select((t, i) => GetAutoEmbed(t, i + 1)).ToList();

        if (dress.UnitSkill != null)
        {
            fields.Add(new EmbedFieldBuilder()
            {
                Name = $"{_db._icons[dress.UnitSkill.Icon].Emote} Unit Skill [Level 21]",
                Value = GetSkillDescription(dress.UnitSkill)
            });
        }

        var e = new EmbedBuilder()
        {
            Author = author,
            Description = description,
            Fields = fields,
            ThumbnailUrl = dress.ThumbUrl
        };
        
        return e.Build();
    }

    public Embed DressEmbedMisc(Dress dress, int rb = 0)
    {
        var author = new EmbedAuthorBuilder()
        {
            IconUrl = dress.ThumbUrl,
            Name = $"{dress.BaseRarity}★ {dress.Name} [{dress.Pool}]"
        };

        var baseSpeed = dress.Stats[rb][5];
        
        var s = new List<string>()
        {
            "**Relevant Speeds**```"
        };
        s.AddRange(_relevantSpeeds.Select(speed => $"Speed × {speed}".PadRight(15) + $"= {Math.Floor(baseSpeed * speed)}"));
        s[^1] += "```";
        
        if(dress.Notes != "")
            s.Add($"**Notes**\n{dress.Notes}");

        var e = new EmbedBuilder()
        {
            Author = author,
            ThumbnailUrl = dress.ThumbUrl,
            Description = string.Join("\n", s)
        };

        return e.Build();
    }
    
    private EmbedFieldBuilder GetActEmbed(Act act, int actNumber, bool cx = false)
    {
        return new EmbedFieldBuilder()
        {
            //TODO: Make effect icon strings instead of int
            Name = cx ? $"{_db._icons[act.Skill.Icon].Emote} CA [{act.Cost}AP]{act.Name}" :
                $"{_db._icons[act.Skill.Icon].Emote} ACT{actNumber} [{act.Cost}AP]{act.Name}",
            Value = GetSkillDescription(act.Skill)
        };
    }

    private EmbedFieldBuilder GetAutoEmbed(Skill skill, int skillNumber)
    {
        var unlock = GetAutoskillUnlock(skillNumber);

        return new EmbedFieldBuilder()
        {
            Name = $"Auto-skill {skillNumber} {unlock}",
            Value = GetSkillDescription(skill, true)
        };
    }

    private string GetSkillDescription(Skill skill, bool firstLevel = false)
    {
        var targets = new Dictionary<string, List<Tuple<string,string>>>();
        foreach (var effect in skill.Effects)
        {
            if (!targets.ContainsKey(effect.Target))
                targets[effect.Target] = new List<Tuple<string, string>>();
            targets[effect.Target].Add(new Tuple<string, string>(
                $"{_db._icons[effect.GetEffect().IconId.ToString()].Emote} {effect.Description(firstLevel: firstLevel)}", 
                effect.ExtraDescription()));
        }

        var actDesc = "";
        foreach (var target in targets.Keys)
        {
            actDesc += "\n";
            var descs = targets[target].Select(t => t.Item1).ToList();
            var extraDescs = targets[target].Select(t => t.Item2).ToList();
            actDesc += $"{string.Join(" & ", descs)}: {target}";
            if(string.Join("", extraDescs).Length == 0)
                continue;
        }

        return actDesc;
    }
    
    public class DressMenuId
    {
        public const string MenuPrefix = "d";
        public readonly string DressId;
        public readonly int Remake;
        public readonly int MenuId;
        
        public DressMenuId(string dressId, int remake, int menuId)
        {
            DressId = dressId;
            Remake = remake;
            MenuId = menuId;
        }

        public DressMenuId(string dressMenuId)
        {
            var split = dressMenuId.Split('_');

            if (split[0] != MenuPrefix)
            {
                throw new ArgumentException("Attempted to create DressMenuId component from non DressMenuId string");
            }
            
            DressId = split[1];
            if (!int.TryParse(split[2], out Remake))
            {
                Remake = 0;
            }
            
            if (!int.TryParse(split[3], out MenuId))
            {
                MenuId = 0;
            }
        }

        public override string ToString()
        {
            return $"{MenuPrefix}_{DressId}_{Remake}_{MenuId}";
        }

        public string ToCustomString(int rb = -1, int menuId = -1)
        {
            if (menuId == -1)
                menuId = MenuId;
            if (rb == -1)
                rb = Remake;
            int ds;
            
            return $"{MenuPrefix}_{DressId}_{rb}_{menuId}";
        }
    }
    
    private static string GetAutoskillUnlock(int i)
    {
        return i switch
        {
            1 => "[Default]",
            2 => "[Rank 4]",
            3 => "[Rank 9 Panel]",
            4 => "[Rank 9 Panel]",
            _ => ""
        };
    }
}