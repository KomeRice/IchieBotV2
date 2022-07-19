using Discord;
using IchieBotData.Legacy;
using IchieBotV2.Services;

namespace IchieBotV2.Utils
{
    public class EmbedGenerator
    {
        // TODO: Refine async scheme
        private readonly DatabaseService _db;
        private static readonly List<string> _menuEntries = new List<string>() {"Overview", "Skills"};
        
        public EmbedGenerator(DatabaseService db)
        {
            _db = db;
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
                                curRb ? $"disabled{disabledIndex++}" : dressId + $"_1{i}{bits[2]}",
                                curRb ? ButtonStyle.Success : ButtonStyle.Primary, disabled: curRb);
                        }
                        rows.Add(rbRow);
                    }

                    var menuRow = new ActionRowBuilder();

                    for (var i = 0; i < _menuEntries.Count; i++)
                    {
                        var curMenu = i == bits[2] - '0';

                        menuRow.WithButton(_menuEntries[i], 
                            curMenu ? $"disabled{disabledIndex++}": dressId + $"_1{rb}{i}",
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
            var tagFooter = "";
            /*
            if (dress.RealTagList.Count > 0)
                tagFooter += $"\nTags: {string.Join(",", dress.RealTagList)}";
            */
            var author = new EmbedAuthorBuilder()
            {
                IconUrl = dress.ThumbUrl,
                Name = $"{dress.Rarity}★ {dress.Name} [{dress.Pool}]"
            };
            var desc = $"{_db.GetEmoteFromIcon(dress.Element.Name)} {FirstCharToUpper(dress.Element.Name)} | " +
                       $"{_db.GetEmoteFromIcon(dress.Row.Name)} {FirstCharToUpper(dress.Row.Name)} | " +
                       $"{TypeToDisplayString(dress.Special)}";

            List<int> stats;
            if (rb == 0 || !_db.Calculator.HasRemake(dress.DressId[2..]))
                stats = dress.MaxStats;
            else
            {
                stats = await _db.GetFromCache(dress.DressId[2..], rb) ?? throw new InvalidOperationException();
            }

            var embedFieldBuilders = new List<EmbedFieldBuilder>()
            {
                new EmbedFieldBuilder()
                {
                    Name = "Maximal Stats",
                    Value = $"```Combined: {stats[0]}\n" +
                            $"HP: {stats[1]}\n" +
                            $"ACT Power: {stats[2]}\n" +
                            $"NormDef: {stats[3]}\n" +
                            $"SpDef: {stats[4]}\n" +
                            $"Agility: {stats[5]}```"
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

            var description = dress.Aliases.Count > 0 ? $"Aliases: *{string.Join(", ", dress.Aliases)}*" : "*No aliases*";
            var fields = new List<EmbedFieldBuilder>();
            for (var i = 0; i < dress.Abilities.Count; i++)
            {
                fields.Add(new EmbedFieldBuilder()
                {
                    Name = _db.GetEmoteFromIcon(dress.Abilities[i].AbilityIcon.Name) + $" Auto-skill {i + 1} {GetAutoskillUnlock(i)}",
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
        
        private static string FirstCharToUpper(string s)
        {
            return s.Length switch
            {
                0 => "",
                1 => char.ToUpper(s[0]).ToString(),
                _ => char.ToUpper(s.First()) + s[1..]
            };
        }

        private static string TypeToDisplayString(bool type)
        {
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