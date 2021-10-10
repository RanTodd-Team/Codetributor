using Microsoft.AspNetCore.Mvc;
using Codetributor.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Octokit;
using DSharpPlus.Entities;

namespace Codetributor.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly ConfigJson _config;
        private readonly InteractionStore _store;

        public AuthorizationController(ConfigJson config, InteractionStore store)
        {
            _config = config;
            _store = store;
        }

        [TempData]
        public string InteractionId { get; set; }

        public IActionResult Index(ulong interaction)
        {
            InteractionId = interaction.ToString();
            return Redirect($"https://github.com/login/oauth/authorize" +
                $"?client_id={_config.ClientId}" +
                "&scope=repo:status" +
                $"&redirect_uri={_config.Host}Authorization/Callback" +
                "&allow_signup=false");
        }

        public async Task<IActionResult> Callback(string code)
        {
            HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            var response = await client.PostAsync("https://github.com/login/oauth/access_token" +
                $"?client_id={_config.ClientId}" +
                $"&client_secret={Environment.GetEnvironmentVariable("CODETRIBUTOR_GITHUB_SECRET")}" +
                $"&code={code}",
                null);
            var json = await response.Content.ReadAsStringAsync();
            JToken parsedJson = JToken.Parse(json);
            string accessToken = parsedJson["access_token"].Value<string>();

            GitHubClient ghClient = new(new Octokit.ProductHeaderValue("Codetributor"));
            ghClient.Credentials = new Credentials(accessToken);

            var currentUser = await ghClient.User.Current();

            bool any = false;

            foreach (var repo in _config.Repos)
            {
                var commits = await ghClient
                    .Repository
                    .Commit
                    .GetAll(
                        repo.Owner,
                        repo.Name,
                        new CommitRequest() { Author = currentUser.Login });

                if (commits.Count > 0)
                {
                    any = true;
                    break;
                }
            }
            ulong iId = ulong.Parse(InteractionId);

            if (!any)
            {
                var ctx = _store.GetInteraction(iId);
                try
                {
                    var builder = new DiscordFollowupMessageBuilder()
                        .WithContent("Sorry, but I cannot find any commits authored by you.")
                        .AsEphemeral(true);
                    await ctx.FollowUpAsync(builder);
                }
                catch
                {
                    try
                    {
                        await ctx.Member.SendMessageAsync("Sorry, but I cannot find any commit authored by you.");
                    }
                    catch
                    {
                        await ctx.Guild.GetChannel(_config.Discord.ChannelId).SendMessageAsync("Sorry, but I cannot find any commits authored by you.");
                    }
                }
                return RedirectToAction("Completed");
            }
            else
            {
                var ctx = _store.GetInteraction(iId);
                DiscordRole role = ctx.Guild.GetRole(_config.Discord.RoleId);
                await ctx.Member.GrantRoleAsync(role);
                try
                {
                    var builder = new DiscordFollowupMessageBuilder()
                        .WithContent("Thank you. You are now a verified codetributor!")
                        .AsEphemeral(true);
                    await ctx.FollowUpAsync(builder);
                }
                catch
                {
                    try
                    {
                        await ctx.Member.SendMessageAsync("Thank you. You are now a verified codetributor!");
                    }
                    catch
                    {
                        await ctx.Guild.GetChannel(_config.Discord.ChannelId).SendMessageAsync("Thank you. You are now a verified codetributor!");
                    }
                }
                return RedirectToAction("Completed");
            }
        }

        public IActionResult Completed()
        {
            return View();
        }
    }
}
