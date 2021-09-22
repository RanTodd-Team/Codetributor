using Codetributor.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System.Threading.Tasks;

namespace Codetributor.Commands
{
    public class UserCommands : ApplicationCommandModule
    {
        private readonly ConfigJson _config;
        private readonly InteractionStore _store;

        public UserCommands(ConfigJson config, InteractionStore store)
        {
            _config = config;
            _store = store;
        }

        [SlashCommand("verify", "Starts commit verification.")]
        [SlashRequireBotPermissions(Permissions.ManageRoles)]
        public async Task Verify(InteractionContext ctx)
        {
            _store.AddInteraction(ctx);
            var builder = new DiscordInteractionResponseBuilder()
                .WithContent("Hi there!\n" +
                "I heard that you want me to verify your commit status.\n" +
                "To start, press the button below.")
                .AddComponents(new DiscordLinkButtonComponent($"{_config.Host}" +
                "Authorization/" +
                "?interaction=" +
                $"{ctx.InteractionId}",
                "Start verification"))
                .AsEphemeral(true);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
        }
    }
}
