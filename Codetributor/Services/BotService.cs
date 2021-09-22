using Codetributor.Commands;
using Codetributor.Models;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Codetributor.Services
{
    public class BotService : IHostedService
    {
        private readonly ConfigJson _configJson;
        private readonly InteractionStore _store;

        public BotService(ConfigJson configJson, InteractionStore store)
        {
            _configJson = configJson;
            _store = store;
        }

        DiscordClient client;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            DiscordConfiguration configuration = new()
            {
                Token = Environment.GetEnvironmentVariable("CODETRIBUTOR_DISCORD"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.Guilds
            };
            client = new(configuration);

            ServiceCollection serv = new();
            serv.AddSingleton(_configJson);
            serv.AddSingleton(_store);

            var slashCommands = client.UseSlashCommands(new()
            {
                Services = serv.BuildServiceProvider()
            });

            slashCommands.RegisterCommands<UserCommands>(_configJson.Discord.GuildId);

            await client.ConnectAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            client?.Dispose();
            return Task.CompletedTask;
        }
    }
}
