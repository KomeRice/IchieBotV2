using Discord;
using Discord.Interactions;
using IchieBotData.Legacy;
using IchieBotV2.Services;
using IchieBotV2.Utils;

namespace IchieBotV2.Modules;

public class DressLegacyModule : InteractionModuleBase<SocketInteractionContext>
{
    private DatabaseService Database { get; set; }
    private EmbedGenerator EmbedGenerator { get; set; }
    
    public DressLegacyModule(DatabaseService db, EmbedGenerator embedGenerator)
    {
        Database = db;
        EmbedGenerator = embedGenerator;
    }

    [SlashCommand("dresslegacy", "Shows a Stage Girl in Legacy format")]
    public async Task LegacySearch([Autocomplete(typeof(DressCompleteHandler)),
                                    Discord.Interactions.Summary("query","Display the dress page matching the search result")]string query)
    {
        StageGirl d;
        if (query.Any(char.IsLetter))
        {
            var results = Database.LegacySearch(query);
            var stageGirls = results.ToList();
            if (stageGirls.Count == 1)
            {
                d = stageGirls[0];
            }
            else
            {
                await RespondAsync("Not implemented yet");
                return;
            }
        }
        else
        {
            try
            {
                d = Database.GetFromDressId(query);

            }
            catch (ArgumentNullException e)
            {
                await RespondAsync("Nothing found");
                return;
            }
        }
        var embed = EmbedGenerator.LegacyToEmbedOverview(d);
        var builder = new ComponentBuilder()
            .WithButton("Overview", query + "_100", disabled: true, style: ButtonStyle.Success)
            .WithButton("Skills", query + "_101");
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
                if (curIn is null or "")
                    return AutocompletionResult.FromSuccess();
                var o = await _databaseService.AutoCompleteFilter(curIn);
                return AutocompletionResult.FromSuccess(o);
            }
            return AutocompletionResult.FromSuccess();
        }
    }
}