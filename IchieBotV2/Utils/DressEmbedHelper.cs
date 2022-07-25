using Discord;
using IchieBotData.Legacy;
using IchieBotV2.Services;

namespace IchieBotV2.Utils
{
    public class DressEmbedHelper
    {
        // TODO: Refine async scheme
        private readonly DatabaseService _db;
        private readonly RankingService _ranking;

        private static readonly List<string> _menuEntries = new List<string>() { "Overview", "Skills" };
        public const int MaxPageSize = 14;

        public DressEmbedHelper(DatabaseService db, RankingService ranking)
        {
            _db = db;
            _ranking = ranking;
        }

        public async Task<List<ActionRowBuilder>> LegacyEmbedMenu(string uniqueId)
        {
            var split = uniqueId.Split('_');
            var dressId = split[0];
            var bits = split[1];

            switch (bits[0])
            {
                case '1':
                    var rb = bits[1] - '0';
                    // Used because custom IDs cannot be duplicated
                    var disabledIndex = 0;

                    var rows = new List<ActionRowBuilder>();

                    if (_db.Calculator.HasRemake(dressId))
                    {
                        var rbRow = new ActionRowBuilder();
                        for (var i = 0; i < 5; i++)
                        {
                            var curRb = i == rb;

                            rbRow.WithButton($"RB{i}",
                                curRb ? $"disabled-{disabledIndex++}" : $"dresslegacy-{dressId}_1{i}{bits[2]}",
                                curRb ? ButtonStyle.Success : ButtonStyle.Primary, disabled: curRb);
                        }
                        rows.Add(rbRow);
                    }

                    var menuRow = new ActionRowBuilder();

                    for (var i = 0; i < _menuEntries.Count; i++)
                    {
                        var curMenu = i == bits[2] - '0';

                        menuRow.WithButton(_menuEntries[i],
                            curMenu ? $"disabled-{disabledIndex++}" : $"dresslegacy-{dressId}_1{rb}{i}",
                            curMenu ? ButtonStyle.Success : ButtonStyle.Primary, disabled: curMenu);
                    }
                    rows.Add(menuRow);

                    return rows;
                default:
                    await Program.LogAsync(new LogMessage(LogSeverity.Error, "menugen", "Got invalid first bit"));
                    return new List<ActionRowBuilder>();
            }
        }

        public async Task<Embed> LegacyToEmbedOverview(StageGirl dress, int rb = 0)
        {
            var tagFooter = dress.RealTagList.Count > 0 ? $"\nTags: {string.Join(", ", dress.RealTagList)}" : "";
            
            var author = new EmbedAuthorBuilder()
            {
                IconUrl = dress.ThumbUrl,
                Name = $"{dress.Rarity}★ {dress.Name} [{dress.Pool}]"
            };
            var desc = $"{_db.GetEmoteFromIcon(dress.Element.Name)} {FirstCharToUpper(dress.Element.Name)} | " +
                       $"{_db.GetEmoteFromIcon(dress.Row.Name)} {FirstCharToUpper(dress.Row.Name)} | " +
                       $"{TypeToDisplayString(dress.Special)}";
            
            if (!_db.DictComplements.ContainsKey(dress.DressId[2..]))
                await Program.LogAsync(new LogMessage(LogSeverity.Error, "embedDressGen",
                    "A dress failed to find its complement in Complement.json"));
            else
            {
                var complement = _db.DictComplements[dress.DressId[2..]];
                desc += $"\nCost: {complement.Cost}" +
                        $"\nReleased (JP): <t:{complement.Release["ja"]}>";
                if(complement.Release["ww"] is not null) 
                    desc += $" | (WW): <t:{complement.Release["ww"]}>";
            }
            

            List<int> stats;
            if (rb == 0 || !_db.Calculator.HasRemake(dress.DressId[2..]))
                stats = dress.MaxStats;
            else
            {
                stats = await _db.GetFromReproductionCache(dress.DressId[2..], rb) ??
                        throw new InvalidOperationException();
            }

            var ranks = _ranking.GetRanks(dress.DressId[2..], rb);

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
                            "Row Position".PadRight(18) + _ranking.GetPositionIndex(dress.DressId[2..]) + "\n" +
                            "Ranked Frontmost to Backmost```"
                }
            };

            for (var i = 0; i < dress.Moves.Count; i++)
            {
                var curMove = dress.Moves[i];
                embedFieldBuilders.Add(new EmbedFieldBuilder()
                {
                    Name = $"{_db.GetEmoteFromIcon(curMove.AttackIcon.Name)} ACT{i + 1} [{curMove.Cost}AP]{curMove.Name}",
                    Value = curMove.Description
                });
            }

            embedFieldBuilders.Add(new EmbedFieldBuilder()
            {
                Name = $"{_db.GetEmoteFromIcon(dress.Climax.AttackIcon.Name)} CA [{dress.Climax.Cost}AP]{dress.Climax.Name}",
                Value = dress.Climax.Description
            });

            if (dress.Entry != null)
            {
                embedFieldBuilders.Add(new EmbedFieldBuilder()
                {
                    Name = $"{_db.GetEmoteFromIcon(dress.Entry.AbilityIcon.Name)} Entry ACT",
                    Value = $"{dress.Entry.Description}"
                });
            }

            var e = new EmbedBuilder()
            {
                Author = author,
                Description = desc,
                Color = GetColor(dress.Element),
                Fields = embedFieldBuilders,
                ThumbnailUrl = dress.ThumbUrl,
                Footer = new EmbedFooterBuilder()
                {
                    Text = tagFooter
                }
            };
            return e.Build();
        }

        public Embed LegacyToEmbedSkills(StageGirl dress)
        {
            var author = new EmbedAuthorBuilder()
            {
                IconUrl = dress.ThumbUrl,
                Name = $"{dress.Rarity}★ {dress.Name} [{dress.Pool}]"
            };

            var description = dress.Aliases.Count > 0
                ? $"Aliases: *{string.Join(", ", dress.Aliases)}*"
                : "*No aliases*";
            var fields = new List<EmbedFieldBuilder>();
            for (var i = 0; i < dress.Abilities.Count; i++)
            {
                fields.Add(new EmbedFieldBuilder()
                {
                    Name = _db.GetEmoteFromIcon(dress.Abilities[i].AbilityIcon.Name) +
                           $" Auto-skill {i + 1} {GetAutoskillUnlock(i)}",
                    Value = dress.Abilities[i].Description
                });
            }

            fields.Add(new EmbedFieldBuilder()
            {
                Name = _db.GetEmoteFromIcon(dress.UnitSkill.AbilityIcon.Name) + " Unit Skill [Level 21]",
                Value = dress.UnitSkill.Description
            });

            var e = new EmbedBuilder()
            {
                Author = author,
                Description = description,
                Fields = fields,
                Color = GetColor(dress.Element),
                ThumbnailUrl = dress.ThumbUrl
            };

            return e.Build();
        }

        public Embed MultiresultEmbed(List<StageGirl> results, int page = 0)
        {
            var dressDisplay = results.GetRange(page * MaxPageSize,
                Math.Min(MaxPageSize, results.Count - page * MaxPageSize));

            var strList = dressDisplay.Select(d => $"\t**{d.Rarity}★ {_db.GetEmoteFromIcon(d.Element.Name)} " +
                                                   $"{_db.GetEmoteFromIcon(d.Row.Name)} " +
                                                   $"{TypeToDisplayString(d.Special, true)}{d.Name}**").ToList();
            var desc = string.Join("\n", strList);
            var title = $"{results.Count} results";
            if (desc.Length > 2047)
                desc = CompressEmotes(desc);

            var e = new EmbedBuilder()
            {
                Title = title,
                Description = desc,
                Footer = new EmbedFooterBuilder()
                {
                    Text = results.Count > MaxPageSize ? 
                        $"Page {page + 1} / {Math.Ceiling(results.Count / (double) MaxPageSize)}"
                        : ""
                }
            }.Build();
            return e;
        }

        public static ActionRowBuilder? MultiresultMenu(string uniqueId, int resultCount)
        {
            if (resultCount <= MaxPageSize)
                return null;
            
            var split = uniqueId.Split("_");
            var prefix = $"multdress-{string.Join("_", split.SkipLast(1))}";

            var page = Convert.ToInt32(split.Last());

            var lastPage = Math.Ceiling(resultCount / (double)MaxPageSize);

            var buttons = new ActionRowBuilder();
            if (page - 1 >= 0)
                buttons.WithButton("Previous page", $"{prefix}_{page - 1}", emote: new Emoji("\U000025C0"));
            if (page + 2 <= lastPage)
                buttons.WithButton("Next page", $"{prefix}_{page + 1}", emote: new Emoji("\U000025B6"));

            return buttons;
        }

        private static string CompressEmotes(string str)
        {
            str = str.Replace("<:physical:670777678676099081>", "[N]");
            str = str.Replace("<:special:670777678483161125>", "[S]");
            str = str.Replace("<:icon_position_front:670780625657266186>", "[-->]");
            str = str.Replace("<:icon_position_middle:670780625581506621>", "[->-]");
            str = str.Replace("<:icon_position_back:670780625560666136>", "[>--]");
            str = str.Replace("<:clouds:670777678739144725>", "[Clou]");
            str = str.Replace("<:cosmos:670777678697201708>", "[Spac]");
            str = str.Replace("<:moon:670777678693138432>", "[Moon]");
            str = str.Replace("<:snow:670777678483161102>", "[Snow]");
            str = str.Replace("<:flower:670777678676230174>", "[Flow]");
            str = str.Replace("<:wind:670777678764179478>", "[Wind]");
            str = str.Replace("<:dream:670777678772830219>", "[Drea]");
            return str;
        }

        private static string FirstCharToUpper(string s)
        {
            return s.Length switch
            {
                0 => "",
                1 => char.ToUpper(s[0]).ToString(),
                _ => char.ToUpper(s.First()) + s[1..]
            };
        }

        private static string TypeToDisplayString(bool type, bool emoteOnly = false)
        {
            if (emoteOnly)
                return type ? "<:special:670777678483161125>" : "<:physical:670777678676099081>";
            return type ? "<:special:670777678483161125> Special" : "<:physical:670777678676099081> Normal";
        }

        private static Color GetColor(Icon icon)
        {
            switch (icon.Name)
            {
                case "cloud":
                    return new Color(248, 95, 139);
                case "cosmos":
                    return new Color(130, 79, 154);
                case "moon":
                    return new Color(250, 169, 0);
                case "flower":
                    return new Color(236, 77, 73);
                case "wind":
                    return new Color(55, 174, 74);
                case "snow":
                    return new Color(0, 122, 180);
                case "dream":
                    return new Color(81, 89, 112);
                default:
                    Program.LogAsync(new LogMessage(LogSeverity.Error, "GetClr",
                        "GetColor method returned default color, that's not normal"));
                    return Color.Default;
            }
        }

        private static string GetAutoskillUnlock(int i)
        {
            return i switch
            {
                0 => "[Default]",
                1 => "[Rank 4]",
                2 => "[Rank 9 Panel]",
                3 => "[Rank 9 Panel]",
                _ => ""
            };
        }
    }
}