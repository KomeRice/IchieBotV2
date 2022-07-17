using Discord;
using IchieBotData.Legacy;
using IchieBotV2.Services;
using OldJsonFormatParser.LegacyClass;

namespace IchieBotV2.Utils
{
    public class EmbedGenerator
    {
        private readonly DatabaseService _db;
        
        public EmbedGenerator(DatabaseService db)
        {
            _db = db;
        }
        
        public Embed LegacyToEmbedOverview(StageGirl dress)
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

            var embedFieldBuilders = new List<EmbedFieldBuilder>()
            {
                new EmbedFieldBuilder()
                {
                    Name = "Maximal Stats",
                    Value = $"```Combined: {dress.MaxStats[0]}\n" +
                            $"HP: {dress.MaxStats[1]}\n" +
                            $"ACT Power: {dress.MaxStats[2]}\n" +
                            $"NormDef: {dress.MaxStats[3]}\n" +
                            $"SpDef: {dress.MaxStats[4]}\n" +
                            $"Agility: {dress.MaxStats[5]}```"
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
    }
}