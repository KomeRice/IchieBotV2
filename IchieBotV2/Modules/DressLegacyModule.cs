using Discord;
using Discord.Interactions;
using IchieBotData.Common;
using IchieBotData.Legacy;
using IchieBotV2.Services;
using IchieBotV2.Utils;

namespace IchieBotV2.Modules;

public class DressLegacyModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DatabaseLegacyService _db;
    private readonly DressLegacyEmbedHelper _legacyEmbedHelper;
    
    public DressLegacyModule(DatabaseLegacyService db, DressLegacyEmbedHelper legacyEmbedHelper)
    {
        _db = db;
        _legacyEmbedHelper = legacyEmbedHelper;
    }

    [SlashCommand("dresslegacy", "Shows a Stage Girl in Legacy format")]
    public async Task LegacySearch([Autocomplete(typeof(DressCompleteHandler)),
                                    Summary("name","Attempts to match name or aliases")]string name)
    {
        if (name.Length is < 3 or > 60)
        {
            await RespondAsync(embed: new EmbedBuilder().WithDescription("Search name must contain at least 3 characters and less than 80 characters")
                .Build());
            return; 
        }
        
        StageGirl d;
        if (name.Any(char.IsLetter))
        {
            var uniqueId = DatabaseLegacyService.GetUniqueId(name);
            var stageGirls = _db.TrySearch(uniqueId);
            switch (stageGirls.Count)
            {
                case 0:
                    await RespondAsync(embed: new EmbedBuilder().WithDescription("No dress found matching search criteria")
                        .Build());
                    return;
                case 1:
                    d = stageGirls[0];
                    break;
                default:
                    var e = _legacyEmbedHelper.MultiresultEmbed(stageGirls);
                    var menu = DressLegacyEmbedHelper.MultiresultMenu(uniqueId + "_0", stageGirls.Count);

                    if (menu is not null)
                    {
                        var multBuilder = new ComponentBuilder().AddRow(menu);
                        await RespondAsync(embed: e, components: multBuilder.Build());
                        return;
                    }

                    await RespondAsync(embed: e);
                    return;
            }
        }
        else
        {
            try
            {
                d = _db.GetFromDressId(name);
            }
            catch (KeyNotFoundException)
            {
                await RespondAsync(embed: new EmbedBuilder().WithDescription("No dress found matching search criteria")
                    .Build());
                return;
            }
        }
        
        // TODO: Dedicated message builder
        var embed = await _legacyEmbedHelper.LegacyToEmbedOverview(d);
        var rows = await _legacyEmbedHelper.LegacyEmbedMenu(d.DressId[2..] + "_100");

        var builder = new ComponentBuilder().WithRows(rows);


        await RespondAsync(embed: embed, components: builder.Build());
    }

    [SlashCommand("search", "Search for one or multiple cards with finer control")]
    public async Task LegacySearchAttribute(
        [Summary("name", "Attempts to match name or aliases")]
        string name = "",
        [Summary("element", "Target Element")] Element? element = null,
        [Summary("row", "Target Row")] Row? row = null,
        [Summary("pool", "Target Pool")] Pool? pool = null,
        [Summary("cost", "Target Cost")] Cost? cost = null,
        [Summary("rarity", "Target Rarity")] Rarity? rarity = null,
        [Summary("type", "Target Attack Type")] AttackType? type = null,
        [Summary("school", "Target School")] School? school = null)
    {
        if (element is null && row is null && pool is null && cost is null
            && rarity is null && type is null && school is null)
        {
            if (name == "")
            {
                await RespondAsync(embed: new EmbedBuilder().WithDescription("At least one argument must be filled out")
                    .Build());
                return;
            }
            if (name.Length is > 0 and < 3 or > 60)
            {
                await RespondAsync(embed: new EmbedBuilder().WithDescription("Search name must contain at least 3 characters and less than 80 characters")
                    .Build());
                return; 
            } 
        }

        var uniqueId = DatabaseLegacyService.GetUniqueId(name, element, row, pool, cost, rarity, type, school);
        var dressList = _db.TrySearch(uniqueId);

        switch (dressList.Count)
        {
            case 0:
                await RespondAsync(embed: new EmbedBuilder().WithDescription("No dress found matching search criteria")
                    .Build());
                return;
            case 1:
            {
                var d = dressList.First();
                var embed = await _legacyEmbedHelper.LegacyToEmbedOverview(d);
                var rows = await _legacyEmbedHelper.LegacyEmbedMenu(d.DressId[2..] + "_100");

                var builder = new ComponentBuilder().WithRows(rows);

                await RespondAsync(embed: embed, components: builder.Build());
                return;
            }
        }

        var e = _legacyEmbedHelper.MultiresultEmbed(dressList);
        var menu = DressLegacyEmbedHelper.MultiresultMenu(uniqueId + "_0", dressList.Count);
        if (menu is not null)
        {
            var multBuilder = new ComponentBuilder().AddRow(menu);
            await RespondAsync(embed: e, components: multBuilder.Build());
            return;
        }

        await RespondAsync(embed: e);
    }

    public class DressCompleteHandler : AutocompleteHandler
    {
        private readonly DatabaseLegacyService _databaseLegacyService;
        public DressCompleteHandler(DatabaseLegacyService databaseLegacyService)
        {
            _databaseLegacyService = databaseLegacyService;
        }
        
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            if (autocompleteInteraction.Data.Current.Name == "name" && autocompleteInteraction.Data.Current.Focused)
            {
                var curIn = autocompleteInteraction.Data.Current.Value.ToString();
                if (curIn is null)
                    return AutocompletionResult.FromSuccess();
                var o = _databaseLegacyService.AutoCompleteFilter(curIn);
                return AutocompletionResult.FromSuccess(o);
            }
            return AutocompletionResult.FromSuccess();
        }
    }
}