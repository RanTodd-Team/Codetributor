using DSharpPlus.SlashCommands;
using System.Collections.Generic;

namespace Codetributor.Models
{
    public class InteractionStore
    {
        private readonly Dictionary<ulong, InteractionContext> _list = new();

        public void AddInteraction(InteractionContext ctx)
            => _list.Add(ctx.InteractionId, ctx);

        public InteractionContext GetInteraction(ulong id)
            => _list.GetValueOrDefault(id);
    }
}
