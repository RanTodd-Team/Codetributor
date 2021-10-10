namespace Codetributor.Models
{
    public class DiscordConfig
    {
        public ulong GuildId { get; set; }

        public ulong RoleId { get; set; }
    }

    public class RepoConfig
    {
        public string Owner { get; set; }

        public string Name { get; set; }
    }

    public class ConfigJson
    {
        public DiscordConfig Discord { get; set; }

        public RepoConfig[] Repos { get; set; }

        public string ClientId { get; set; }

        public string Host { get; set; }
    }
}
