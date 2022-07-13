using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace IchieBotV2.Services;


public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _commands;
    private readonly IServiceProvider _services;
    
    public CommandHandler(DiscordSocketClient cl, InteractionService cm, IServiceProvider s)
    {
        _client = cl;
        _commands = cm;
        _services = s;
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
        switch (component.Data.CustomId)
        {
            case "next":
                await component.UpdateAsync(x =>
                {
                    x.Content = "CLICKED NEXT";
                });
                break;
            case "prev":
                await component.UpdateAsync(x =>
                {
                    x.Content = "CLICKED PREV";
                });
                break;
        }
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