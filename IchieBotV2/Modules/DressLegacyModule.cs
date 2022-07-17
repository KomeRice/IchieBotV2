using Discord;
using Discord.Commands;
using Discord.Interactions;
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
                                    Discord.Interactions.Summary("InternalID","Display the dress page matching the id")]string id)
    {
        try
        {
            var d = Database.GetFromDressId(id);
            var e = EmbedGenerator.LegacyToEmbedOverview(d);
            var builder = new ComponentBuilder()
                .WithButton("Overview", id + "_100", disabled: true)
                .WithButton("Skills", id + "_101");
            await RespondAsync(embed: e, components: builder.Build());
        }
        catch (ArgumentNullException e)
        {
            await RespondAsync("Nothing found");
        }
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
            if (autocompleteInteraction.Data.Current.Name == "internal-id" && autocompleteInteraction.Data.Current.Focused)
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