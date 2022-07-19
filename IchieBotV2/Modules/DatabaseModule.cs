using Discord;
using Discord.Interactions;
using IchieBotV2.Services;

namespace IchieBotV2.Modules;

public class DatabaseModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DatabaseService _db;

    public DatabaseModule(DatabaseService db)
    {
        _db = db;
    }

    [RequireOwner, SlashCommand("buildrbcache", "Builds RB Cache")]
    public async Task BuildRbCacheCommand()
    {
        await RespondAsync("Building cache...");
        try
        {
            await _db.BuildReproductionCache();
        }
        catch (Exception e)
        {
            await Program.LogAsync(new LogMessage(LogSeverity.Critical, "buildCache",
                "Caught an error while running buildcache command", e));
            await ReplyAsync("Got an error while trying to build cache");
            return;
        }

        await ReplyAsync("Sucessfully rebuilt cache");
    }
}