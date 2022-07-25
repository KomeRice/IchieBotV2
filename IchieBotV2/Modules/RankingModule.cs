using Discord;
using Discord.Interactions;
using IchieBotV2.Services;
using IchieBotV2.Utils;

namespace IchieBotV2.Modules;

public class RankingModule : InteractionModuleBase<SocketInteractionContext>
{
	private RankingService _ranking;
	private DatabaseService _db;
	private readonly RankingEmbedHelper _embedHelper;
	
	public RankingModule(RankingService ranking, DatabaseService db, RankingEmbedHelper embedHelper)
	{
		_ranking = ranking;
		_db = db;
		_embedHelper = embedHelper;
	}

	[SlashCommand("ranking", "Displays ranking for the given parameter")]
	public async Task Ranking([Discord.Interactions.Summary("stat", "Target stat")] RankingService.Parameter p,
		[Discord.Interactions.Summary("rb", "Target RB Level")] RbLevel rb = RbLevel.RB0)
	{
		if ((int) rb is < 0 or > 4)
		{
			await RespondAsync("RB level must be between 0 and 4.");
			return;
		}

		if (rb != RbLevel.RB0 && p == RankingService.Parameter.RowPosition)
		{
			await ReplyAsync(embed: new EmbedBuilder
			{
				Description = "RB level ignored for Position ranking"
			}.Build());
		}

		var e = await _embedHelper.RankingEmbed(p, rb: (int) rb);
		var menu = _embedHelper.RankingMenu($"{(int)p}_{(int) rb}_0");
		var builder = new ComponentBuilder().AddRow(menu);
		await RespondAsync(embed: e, components: builder.Build());
	}

	public enum RbLevel
	{
		RB0 = 0,
		RB1 = 1,
		RB2 = 2,
		RB3 = 3,
		RB4 = 4
	}
}