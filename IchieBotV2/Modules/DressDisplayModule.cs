using System.ComponentModel;
using Discord;
using Discord.Interactions;
using IchieBotData;
using IchieBotV2.Services;
using IchieBotV2.Utils;

namespace IchieBotV2.Modules;

public class DressDisplayModule : InteractionModuleBase
{
    private readonly DatabaseService _db;
    private readonly DressEmbedHelper _embedHelper;

    public DressDisplayModule(DatabaseService db, DressEmbedHelper helper)
    {
        _db = db;
        _embedHelper = helper;
    }

    [SlashCommand("dress", "xd")]
    public async Task DressCommand(string name)
    {
        if (name.Length is < 3 or > 50)
        {
            await RespondAsync(
                embed: new EmbedBuilder().WithDescription(
                    "Search query cannot be less than 3 characters long or over 50 characters").Build());
            return;
        }

        Dress d;
        if (_db.Dresses.ContainsKey(name))
        {
            d = _db.Dresses[name];
        }
        else
        {
            d = _db.Dresses.Values.First();
        }

        var embed = _embedHelper.DressEmbedOverview(d);
        var rows = await _embedHelper.DressEmbedMenu(new DressEmbedHelper.DressMenuId(d.DressId, 0, 0).ToString());

        var builder = new ComponentBuilder().WithRows(rows);

        await RespondAsync(embed: embed, components: builder.Build());
    }
}