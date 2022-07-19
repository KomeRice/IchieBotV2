using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using IchieBotV2.Utils;

namespace IchieBotV2.Services;


public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    private readonly EmbedGenerator _embedGenerator;
    private readonly DatabaseService _db;
    
    public CommandHandler(DiscordSocketClient cl, InteractionService cm, IServiceProvider s, EmbedGenerator embedGenerator, DatabaseService db)
    {
        _client = cl;
        _commands = cm;
        _services = s;
        _embedGenerator = embedGenerator;
        _db = db;
    }

    public async Task InitializeAsync()
    {
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        _client.InteractionCreated += HandleInteraction;
        _commands.SlashCommandExecuted += SlashCommandExecuted;
        _client.ButtonExecuted += ButtonExecuted;
    }

    private async Task ButtonExecuted(SocketMessageComponent component)
    {
        if (component.User.Id != component.Message.Interaction.User.Id)
        {
            await component.DeferAsync();
            return;
        }

        var options = component.Data.CustomId.Split("_");
        if (options.Length != 2)
        {
            await component.DeferAsync();
            return;
        }

        var id = options[0];
        var page = options[1];
        var rb = page[1] - '0';
        var d = _db.GetFromDressId(id);

        var e = page[2] switch
        {
            '0' => await _embedGenerator.LegacyToEmbedOverview(d, rb),
            '1' => _embedGenerator.LegacyToEmbedSkills(d),
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };

        var menu = await _embedGenerator.LegacyEmbedMenu(component.Data.CustomId);

        var builder = new ComponentBuilder().WithRows(menu);
        
        await component.UpdateAsync(message =>
        {
            message.Embed = e;
            message.Components = builder.Build();
        });
    }

    private static Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
    {
        if (arg3.IsSuccess)
        {
            return Task.CompletedTask;
        }

        switch (arg3.Error)
        {
            case InteractionCommandError.UnknownCommand:
                break;
            case InteractionCommandError.ConvertFailed:
                break;
            case InteractionCommandError.BadArgs:
                break;
            case InteractionCommandError.Exception:
                break;
            case InteractionCommandError.Unsuccessful:
                break;
            case InteractionCommandError.UnmetPrecondition:
                break;
            case InteractionCommandError.ParseFailed:
                break;
            case null:
                break;
            default:
                Console.WriteLine("oops");
                break;
        }

        return Task.CompletedTask;
    }

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, arg);
            await _commands.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}