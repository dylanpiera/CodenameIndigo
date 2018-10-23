using DiscordBot.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [Group("sudo", CanInvokeWithoutSubcommand = true), Aliases("o"), RequireOwner, Hidden]
    internal class Owner
    {
        public async Task ExecuteGroupAsync(CommandContext context, DiscordMember member, string command)
        {
            await context.CommandsNext.SudoAsync(member, context.Channel, command);
        }

        private Dependencies dep;
        public Owner(Dependencies d)
        {
            dep = d;
        }

        [Command("shutdown")]
        public async Task ShutdownAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("Shutting down!");
            dep.Cts.Cancel();
        }

    }
}
