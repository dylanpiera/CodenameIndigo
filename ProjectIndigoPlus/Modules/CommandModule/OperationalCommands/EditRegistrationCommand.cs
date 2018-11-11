using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Entities;
using ProjectIndigoPlus.Modules.HelperModule;
using ProjectIndigoPlus.Modules.ModelModule;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class EditRegistration
    {
        private readonly Dependencies dep;
        public EditRegistration(Dependencies d)
        {
            dep = d;
        }

        [Command("editsignup"),
            Aliases(new[] { "editregistration", "editteam" }),
            Description("Allows you to change your signup for a tournament you already registered to. The tourney must still be accepting signups.")]
        public async Task Command(CommandContext context, string input = "")
        {
            DiscordChannel channel;

            if (!context.Channel.IsPrivate)
            {
                context.RespondAndDelete(new DiscordEmbedBuilder()
                {
                    Color = Bot._config.Color,
                    Title = "Edit Tournament Registration",
                    Description = $"Hey {context.User.Mention}! I'll be sending you a DM with the details :)"
                }, TimeSpan.FromSeconds(20));
                channel = (await context.Member.CreateDmChannelAsync());
            }
            else
            {
                channel = context.Channel;
            }

            Dictionary<int, TourneyModel> tournaments = new Dictionary<int, TourneyModel>();
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand($"SELECT `tournaments`.`tid`,`tournaments`.`tournament`,`tournaments`.`regstart`,`tournaments`.`regend`,`tournaments`.`closure`,`tournaments`.`maxplayers`,`tournaments`.`minplayers` FROM `tournaments` LEFT JOIN `teams` ON `tournaments`.`tid` = `teams`.`tid` WHERE `teams`.`uid` = {context.User.Id} AND `regstart` <= {DateTimeOffset.Now.ToUnixTimeSeconds()} AND `regend` >= {DateTimeOffset.Now.ToUnixTimeSeconds()} ORDER BY `tournaments`.`tid` DESC", conn);

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {

                    while (await reader.ReadAsync())
                    {
                        tournaments.Add(reader.GetInt32("tid"), new TourneyModel()
                        {
                            Tid = reader.GetInt32("tid"),
                            Name = reader.GetString("tournament"),
                            RegStart = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("regstart")),
                            RegEnd = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("regend")),
                            Closure = DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64("closure")),
                            MaxPlayers = reader.GetInt32("maxplayers"),
                            MinPlayers = reader.GetInt32("minplayers")
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "Edit Signup Tournament Retrieval" + "", e.ToString(), DateTime.Now);
            }
            finally
            {
                await conn.CloseAsync();
            }

            if (tournaments.Count == 0)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "No available tournaments",
                    Description = "Aww... it appears there are no tournament signups that you can edit. :cry:\n\n" +
                    "Please keep in mind you can only edit a signup before the tournament starts."
                });
                return;
            }
            #region !! Display Tournament Picker !!
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Pick a tournament",
                Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = "Please select a tournament by sending its number!" }
            };

            builder.Description = "The following tourneys are available:\n\n";
            foreach (TourneyModel tourney in tournaments.Values)
            {
                builder.Description += $"{tourney.Tid}. {tourney.Name} - [{tourney.PlayerCount}/{tourney.MaxPlayers}]\n";
            }

            await channel.SendMessageAsync("", false, builder.Build());
            #endregion

            RetryPicker:
            MessageContext ctx = await dep.Interactivity.WaitForMessageAsync(x =>
            {
                return x.Channel.Id == channel.Id &&
                x.Author.Id == context.User.Id &&
                Regex.IsMatch(x.Content, @"^\d+$");
            }
            );
            if (ctx == null)
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Tourney Selection",
                    Description = $"It appears I lost you :cry:\n To restart type `{Bot._config.Prefix + context.Command.Name}`"
                });
                return;
            }
            else if (ctx.Message.Content.ToLower() == "exit")
            {
                return;
            }

            int.TryParse(ctx.Message.Content, out int tid);

            if (!tournaments.ContainsKey(tid))
            {
                await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Tourney Selection",
                    Description = $"It appears that tournament with ID {ctx.Message.Content} doesn't excist, could you give me one of the numbers listed from one of the tournaments above?"
                });
                goto RetryPicker;
            }

            DiscordMessage confirmMessage = await channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Tourney Signup",
                Description = $"Cool! Let's edit your signup for the {tournaments[tid].Name}!\n\n"
            });
            EditRegistration(context, channel, tid);
        }

        internal static void EditRegistration(CommandContext context, DiscordChannel channel, int tid)
        {
            context.RespondAsync("Not yet implemented.");
        }
    }
}
