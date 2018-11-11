using ProjectIndigoPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using ProjectIndigoPlus.Modules.HelperModule;
using System.Diagnostics;
using System;

namespace ProjectIndigoPlus.Modules.Commands
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

        [Command("relay")]
        public async Task RelayAsync(CommandContext ctx, ulong channel, [RemainingText] string msg)
        {
            await (await ctx.Client.GetChannelAsync(channel)).SendMessageAsync(msg);
        }

        [Command("testcon")]
        public async Task TestConnAsync(CommandContext context)
        {
            //Measure ping with database connection     
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            await context.RespondAsync("Connecting...");
            try
            {
                await DatabaseHelper.GetClosedConnection().OpenAsync();
                await context.RespondAsync($"Connected in `{sw.ElapsedMilliseconds}ms`");
            }
            catch (Exception e)
            {
                await context.RespondAsync($"An error occured after `{sw.ElapsedMilliseconds}`ms "+((e.ToString().Length > 1999) ? e.ToString().Remove(1900) : e.ToString()));
                context.Client.DebugLogger.LogMessage(DSharpPlus.LogLevel.Error,"Connection Test", e.ToString(), DateTime.Now);
            }
            sw.Stop();
        }

    }
}
