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
    private readonly DressLegacyEmbedHelper _dressLegacyEmbedHelper;
    private readonly DressEmbedHelper _dressEmbedHelper;
    private readonly RankingLegacyEmbedHelper _rankingLegacyEmbedHelper;
    private readonly DatabaseLegacyService _dbLegacy;
    private readonly DatabaseService _db;

    public CommandHandler(DiscordSocketClient cl, InteractionService cm, IServiceProvider s,
        DressLegacyEmbedHelper dressLegacyEmbedHelper, DatabaseLegacyService dbLegacy,
        RankingLegacyEmbedHelper rankingLegacyEmbedHelper,
        DressEmbedHelper dressEmbedHelper, DatabaseService db)
    {
        _client = cl;
        _commands = cm;
        _services = s;
        _dressLegacyEmbedHelper = dressLegacyEmbedHelper;
        _dbLegacy = dbLegacy;
        _rankingLegacyEmbedHelper = rankingLegacyEmbedHelper;
        _dressEmbedHelper = dressEmbedHelper;
        _db = db;
    }

    public async Task InitializeAsync()
    {
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

        if (!component.Data.CustomId.Contains('-'))
        {
            var id = new DressEmbedHelper.DressMenuId(component.Data.CustomId);
            var dress = _db.Dresses[id.DressId];

            var embed = id.MenuId switch
            {
                0 => _dressEmbedHelper.DressEmbedOverview(dress, id.Remake),
                1 => _dressEmbedHelper.DressEmbedSkills(dress),
                2 => _dressEmbedHelper.DressEmbedMisc(dress, id.Remake),
                _ => throw new ArgumentOutOfRangeException()
            };

            var menus = await _dressEmbedHelper.DressEmbedMenu(id.ToString());

            var b = new ComponentBuilder().WithRows(menus);

            await component.UpdateAsync(message =>
            {
                message.Embed = embed;
                message.Components = b.Build();
            });
            return;
        }
        
        var split = component.Data.CustomId.Split("-");
        if (split.Length != 2)
        {
            await component.DeferAsync();
            return;
        }
        
        // TODO: Delegate to embed helpers
        var options = split[1].Split("_");
        Embed? e;
        switch (split[0])
        {
            case "dresslegacy":
                var id = options[0];
                var page = options[1];
                var rb = page[1] - '0';
                var d = _dbLegacy.GetFromDressId(id);

                e = page[2] switch
                {
                    '0' => await _dressLegacyEmbedHelper.LegacyToEmbedOverview(d, rb),
                    '1' => _dressLegacyEmbedHelper.LegacyToEmbedSkills(d),
                    '2' => await _dressLegacyEmbedHelper.LegacyToEmbedMisc(d, rb),
                    _ => throw new ArgumentOutOfRangeException(nameof(page))
                };

                var menu = await _dressLegacyEmbedHelper.LegacyEmbedMenu(split[1]);

                var builder = new ComponentBuilder().WithRows(menu);
        
                await component.UpdateAsync(message =>
                {
                    message.Embed = e;
                    message.Components = builder.Build();
                });
                break;
            
            case "multdress":
                var curPage = Convert.ToInt32(options.Last());
                var uniqueId = string.Join("_", options.SkipLast(1).ToList());
                var res = _dbLegacy.TrySearch(uniqueId);
                e = _dressLegacyEmbedHelper.MultiresultEmbed(res, curPage);
                var multMenu = DressLegacyEmbedHelper.MultiresultMenu(split[1], res.Count);
                var multBuilder = new ComponentBuilder().AddRow(multMenu);
                await component.UpdateAsync(message =>
                {
                    message.Embed = e;
                    message.Components = multBuilder.Build();
                });
                break;
            
            case "rank":
                var optionsInt = options.Select(s => Convert.ToInt32(s)).ToArray();
                var rankEmbed = await _rankingLegacyEmbedHelper.RankingEmbed((RankingLegacyService.Parameter)optionsInt[0],
                    optionsInt[2], optionsInt[1]);
                var rankMenu = _rankingLegacyEmbedHelper.RankingMenu(split[1]);
                var rankBuilder = new ComponentBuilder().AddRow(rankMenu);
                await component.UpdateAsync(message =>
                {
                    message.Embed = rankEmbed;
                    message.Components = rankBuilder.Build();
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