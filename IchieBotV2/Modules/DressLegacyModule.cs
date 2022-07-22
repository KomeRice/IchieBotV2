using System.Runtime.InteropServices;
using Discord;
using Discord.Interactions;
using IchieBotData.Legacy;
using IchieBotV2.Services;
using IchieBotV2.Utils;

namespace IchieBotV2.Modules;

public class DressLegacyModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DatabaseService _db;
    private readonly DressEmbedHelper _embedHelper;
    
    public DressLegacyModule(DatabaseService db, DressEmbedHelper embedHelper)
    {
        _db = db;
        _embedHelper = embedHelper;
    }

    [SlashCommand("dresslegacy", "Shows a Stage Girl in Legacy format")]
    public async Task LegacySearch([Autocomplete(typeof(DressCompleteHandler)),
                                    Discord.Interactions.Summary("query","Display the dress page matching the search result")]string query)
    {
        if (query.Length is < 3 and < 80)
        {
            await RespondAsync("Search query must contain at least 3 characters and less than 80 characters");
            return;
        }
        
        StageGirl d;
        if (query.Any(char.IsLetter))
        {
            
            var results = _db.LegacySearch(query);
            var stageGirls = results.ToList();
            switch (stageGirls.Count)
            {
                case 0:
                    await RespondAsync("No dress matching the query has been found");
                    return;
                case 1:
                    d = stageGirls[0];
                    break;
                default:
                    var cleanQuery = DatabaseService.CleanString(query);
                    _db.SearchCache[cleanQuery] = stageGirls.Select(s => s.DressId[2..]).ToList();
                    
                    await RespondAsync("Not implemented yet");
                    return;
            }
        }
        else
        {
            try
            {
                d = _db.GetFromDressId(query);
            }
            catch (ArgumentNullException e)
            {
                await RespondAsync("Nothing found");
                return;
            }
        }
        
        // TODO: Dedicated message builder
        var embed = await _embedHelper.LegacyToEmbedOverview(d);
        var rows = await _embedHelper.LegacyEmbedMenu(d.DressId[2..] + "_100");

        var builder = new ComponentBuilder().WithRows(rows);


        await RespondAsync(embed: embed, components: builder.Build());
    }

    public class DressCompleteHandler : AutocompleteHandler
    {
        private readonly DatabaseService _databaseService;
        public DressCompleteHandler(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }
        
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter, IServiceProvider services)
        {
            if (autocompleteInteraction.Data.Current.Name == "query" && autocompleteInteraction.Data.Current.Focused)
            {
                var curIn = autocompleteInteraction.Data.Current.Value.ToString();
                if (curIn is null)
                    return AutocompletionResult.FromSuccess();
                var o = _databaseService.AutoCompleteFilter(curIn);
                return AutocompletionResult.FromSuccess(o);
            }
            return AutocompletionResult.FromSuccess();
        }
    }
}