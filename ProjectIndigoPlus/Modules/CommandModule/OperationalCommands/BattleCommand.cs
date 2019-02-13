using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Modules.HelperModule;
using System;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class Battle
    {
        #region SQL string
        private const string SQLSTRING = "SELECT `battles`.`round` as Round, `battles`.`player1` as P1ID, `battles`.`player2` as P2ID, P1.showdownusername as P1name, P2.showdownusername as P2name FROM `battles` INNER JOIN `members` AS P1 ON ( P1.`uid` = `battles`.`player1` ) INNER JOIN `members` AS P2 ON ( P2.`uid` = `battles`.`player2` ) WHERE (`player1` = @uid OR `player2` = @uid) AND `battles`.`tid` = @tid";
        #endregion

        [Command("battle"), 
        //Aliases(""),
        Description("Shows your current battle.")]
        public async Task ExecuteCommand(CommandContext context)
        {
            try
            {
                await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":typing:"));
            }
            catch
            {
                await context.Channel.TriggerTypingAsync();
            }

            await context.Channel.SendMessageAsync("", false, await BuildBattleMessage(new DiscordEmbedBuilder()
            {
                Title = $"Current battles for {context.User.Username}",
                Color = Bot._config.Color
            }, context.User.Id));
        }


        [Command("userbattle"),
        //Aliases(""),
        Description("Shows battle for user given as argument.")]
        public async Task ExecuteCommand(CommandContext context, DiscordUser input)
        {
            try
            {
                await context.Message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, ":typing:"));
            }
            catch
            {
                await context.Channel.TriggerTypingAsync();
            }

            try
            {
                await context.Channel.SendMessageAsync("", false, await BuildBattleMessage(new DiscordEmbedBuilder()
                {
                    Title = $"Current battles for {context.User.Username}",
                    Color = Bot._config.Color
                }, input.Id));
            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "Battle Command" + "", e.ToString(), DateTime.Now);

                await context.Channel.SendMessageAsync("An unforseen error occured.");
            }
        }

        private async Task<DiscordEmbed> BuildBattleMessage(DiscordEmbedBuilder builder, ulong uid)
        {
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();

            int tid = (await conn.GetLatestTourneyAsync()).Tid;

            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand(SQLSTRING, conn);
                cmd.Parameters.Add("uid", MySqlDbType.UInt64).Value = uid;
                cmd.Parameters.Add("tid", MySqlDbType.Int32).Value = tid;

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {

                    while (await reader.ReadAsync())
                    {
                        builder.Description += $"**{reader.GetString("P1name")}** VS **{reader.GetString("P2name")}**\n\n";
                    }
                }
            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "Battle Command" + "", e.ToString(), DateTime.Now);
            }
            finally
            {
                await conn.CloseAsync();
            }

            return builder.Build();
        }

    }
}