using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using IchieBotV2.Modules;
using IchieBotV2.Services;
using IchieBotV2.Utils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace IchieBotV2
{
    public class Program
    {
        private DiscordSocketClient _client;
        private IServiceCollection _services;
        private InteractionService _commands;

        public static Task Main(string[] args) => new Program().MainAsync();

        private async Task MainAsync()
        {
            var registerCommands = false;
            try
            {
                var f = await File.ReadAllTextAsync("./config.json");
                var regToken = JObject.Parse(f)["registerCommands"];
                if (regToken == null)
                    throw new FormatException("Failed to fetch config");
                registerCommands = regToken.ToObject<bool>();
            }
            catch (Exception e)
            {
                await LogAsync(new LogMessage(LogSeverity.Error, "entry", "Failed to read config file, defaulting to not registering commands", e));
            }
            
            var key = await GetKey();
            if (key == "")
                return;

            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                var commands = services.GetRequiredService<InteractionService>();
                _client = client;
                _commands = commands;

                client.Log += LogAsync;
                commands.Log += LogAsync;

                if (registerCommands)
                    client.Ready += ReadyAsync;

                await client.LoginAsync(TokenType.Bot, key);
                await client.StartAsync();
                await services.GetRequiredService<CommandHandler>().InitializeAsync();

                await Task.Delay(Timeout.Infinite);
            }
        }

        public static Task LogAsync(LogMessage msg)
        {
            Console.WriteLine(msg);
            return Task.CompletedTask;
        }
        
        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton<CommandHandler>()
                .AddSingleton<DatabaseService>()
                .AddSingleton<EmbedGenerator>()
                .AddSingleton<DressLegacyModule.DressCompleteHandler>()
                .AddSingleton<StatCalculator>()
                .AddSingleton<RankingService>()
                .BuildServiceProvider();
        }

        private async Task ReadyAsync()
        {
#if DEBUG
            try
            {
                var f = await File.ReadAllTextAsync("./secret.json");
                var testGuild = JObject.Parse(f)["testServerSnowflake"];
                if (testGuild == null)
                    throw new FormatException("Could not get test guild snowflake from 'secret.json'");
                var testGuildSnowflake = testGuild.ToObject<ulong>();
                await _commands.RegisterCommandsToGuildAsync(testGuildSnowflake);
            }
            catch (Exception e)
            {
                await LogAsync(new LogMessage(LogSeverity.Critical, "entry", "Failed to register commands to guild.", e));
            }
#else
            await _commands.RegisterCommandsGloballyAsync();
#endif       
        }
        
        private static async Task<string> GetKey()
        {
            try
            {
                var f = await File.ReadAllTextAsync("./secret.json");
#if DEBUG
                const string keyIdentifier = "testBotKey";
#else
                const string keyIdentifier = "releaseBotKey";
#endif
                var key = JObject.Parse(f)[keyIdentifier];
                if (key == null)
                    throw new FormatException("Could not get key from 'secret.json'");
                return key.ToString();
            }
            catch (Exception e)
            {
                await LogAsync(new LogMessage(LogSeverity.Critical, "entry", "Failed to get bot key.", e));
                return "";
            }
        }
    }
}