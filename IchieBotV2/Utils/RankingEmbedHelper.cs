using System.Globalization;
using Discord;
using IchieBotV2.Services;

namespace IchieBotV2.Utils;

public class RankingEmbedHelper
{
	private readonly DatabaseService _db;
	private readonly RankingService _rankingService;
	private const int PageSize = 16;
	public static readonly List<string> Parameters = new List<string>()
		{ "Power Score", "MaxHP", "ACT Power", "NormDef", "SpDef", "Agility" };
	
	public RankingEmbedHelper(DatabaseService db, RankingService rankingService)
	{
		_db = db;
		_rankingService = rankingService;
	}

	public async Task<Embed> RankingEmbed(RankingService.Parameter p, int page = 0, int rb = 0)
	{
		var entries = _rankingService.GetRanking(p, rb);

		var startIndex = page * PageSize;

		var s = new List<string>();
		var count = 1;
		foreach (var rank in entries.Keys)
		{
			foreach (var entry in entries[rank].Split(","))
			{
				if (count < startIndex)
				{
					count++;
					continue;
				}

				var d = _db.GetFromDressId(entry);
				var stat = rb == 0 ? d.MaxStats[(int) p] : (await _db.GetFromCache(d.DressId[2..], rb))![(int) p];
				s.Add($"[{rank}]".PadRight(7) + $"> ({stat}) {d.Name}");
				count++;
				if (count > startIndex + PageSize)
					break;
			}

			if (count > startIndex + PageSize)
				break;
		}

		var e = new EmbedBuilder()
		{
			Title = $"{Parameters[(int)p]} Rankings",
			Description = $"```{string.Join("\n", s)}```",
			Footer = new EmbedFooterBuilder
			{
				Text =
					$"Page {page + 1} / {Math.Ceiling(_rankingService.GetMax(p, rb) / (double)PageSize).ToString(CultureInfo.InvariantCulture)}"
			}
		}.Build();

		return e;
	}

	public ActionRowBuilder RankingMenu(string uniqueId)
	{
		var split = uniqueId.Split("_").Select(s => Convert.ToInt32(s)).ToArray();
		var p = (RankingService.Parameter) split[0];
		var rb = split[1];
		var page = split[2];
		
		var lastPage = Math.Ceiling(_rankingService.GetMax(p, rb) / (double)PageSize);

		var buttons = new ActionRowBuilder();
		if (page - 1 >= 0)
			buttons.WithButton("Previous page", $"rank-{(int) p}_{rb}_{page - 1}", emote: new Emoji("\U000025C0"));
		if (page + 2 <= lastPage)
			buttons.WithButton("Next page", $"rank-{(int)p}_{rb}_{page + 1}", emote: new Emoji("\U000025B6"));

		return buttons;
	}
}