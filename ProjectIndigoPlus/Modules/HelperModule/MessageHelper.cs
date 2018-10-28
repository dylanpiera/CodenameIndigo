using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.HelperModule
{
    public static class MessageHelper
    {
        /// <summary>
        /// Sends a message to the channel and delete it after timeout
        /// </summary>
        /// <param name="channel">The channel where the message should be delivered</param>
        /// <param name="message">The message</param>
        /// <param name="timeout">The timeout after which the message should be deleted</param>
        public static void RespondAndDelete(this DiscordChannel channel, string message, TimeSpan timeout)
        {
            ThreadPool.QueueUserWorkItem(async o =>
            {
                DiscordMessage msg = await channel.SendMessageAsync(message);
                await Task.Delay(timeout);
                await msg.DeleteAsync();
            });
        }

        /// <summary>
        /// Sends a message to the channel and delete it after timeout
        /// </summary>
        /// <param name="channel">The channel where the message should be delivered</param>
        /// <param name="embedBuilder">Discord Embed</param>
        /// <param name="timeout">The timeout after which the message should be deleted</param>
        public static void RespondAndDelete(this DiscordChannel channel, DiscordEmbedBuilder embedBuilder, TimeSpan timeout)
        {
            ThreadPool.QueueUserWorkItem(async o =>
            {
                DiscordMessage msg = await channel.SendMessageAsync("", false, embedBuilder.Build());
                await Task.Delay(timeout);
                await msg.DeleteAsync();
            });
        }

        /// <summary>
        /// Sends a message to the channel and delete it after timeout
        /// </summary>
        /// <param name="context">The context of which the channel is taken to send the message to</param>
        /// <param name="message">The message</param>
        /// <param name="timeout">The timeout after which the message should be deleted</param>
        public static void RespondAndDelete(this CommandContext context, string message, TimeSpan timeout)
        {
            ThreadPool.QueueUserWorkItem(async o =>
            {
                DiscordMessage msg = await context.RespondAsync(message);
                await Task.Delay(timeout);
                await msg.DeleteAsync();
            });
        }

        /// <summary>
        /// Sends a message to the channel and delete it after timeout
        /// </summary>
        /// <param name="context">The context of which the channel is taken to send the message to</param>
        /// <param name="embedBuilder">Discord Embed</param>
        /// <param name="timeout">The timeout after which the message should be deleted</param>
        public static void RespondAndDelete(this CommandContext context, DiscordEmbedBuilder embedBuilder, TimeSpan timeout)
        {
            ThreadPool.QueueUserWorkItem(async o =>
            {
                DiscordMessage msg = await context.RespondAsync("", false, embedBuilder.Build());
                await Task.Delay(timeout);
                await msg.DeleteAsync();
            });
        }
    }
}
