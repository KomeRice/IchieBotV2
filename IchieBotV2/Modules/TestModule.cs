using Discord.Interactions;
using IchieBotV2.Services;

namespace IchieBotV2.Modules;

public class TestModule : InteractionModuleBase<SocketInteractionContext>
{
    private CommandHandler _handler;
    private IServiceProvider _services;

    public TestModule(CommandHandler handler, IServiceProvider services)
    {
        _handler = handler;
        _services = services;
    }

    [SlashCommand("test", "Test command"), RequireOwner]
    public async Task TestCommand(string arg)
    {
        await RespondAsync(arg);
    }
}