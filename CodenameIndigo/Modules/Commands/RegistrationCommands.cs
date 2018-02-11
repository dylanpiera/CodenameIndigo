using CodenameIndigo.Modules.Criteria;
using CodenameIndigo.Modules.Preconditions;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules
{
    public class RegistrationCommands : InteractiveBase
    {
        [Command("register", RunMode = RunMode.Async), Alias(new[] { "signup" }), InSignupPrecon(), UserNotRegisteredPrecon()]
        public async Task Register()
        {
            Player player = new Player() { Id = Context.User.Id, DiscordName = Context.User.Username + "#" + Context.User.Discriminator };

            try
            {
                await ReplyAndDeleteAsync($"Hey {Context.User.Mention}! I've send you a PM with the info!", false, null, TimeSpan.FromSeconds(15));
                await Context.Message.DeleteAsync();
            }
            catch { }

            IDMChannel channel = await Context.User.GetOrCreateDMChannelAsync();
            Tourney tourney = await DatabaseHelper.GetLatestTourneyAsync();
            Restart:
            await channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "☆ BulbaLeague Signups ☆",
                Description = $"Welcome {Context.User.Username}! you are currently signing up for the {tourney.Name}. The tournament will start {DateTimeOffset.FromUnixTimeSeconds(tourney.Regend).ToString("dd-MM-yyyy")}\n\n" +
                $"Please provide your Showdown Username?",
                Footer = new EmbedFooterBuilder() { Text = "If you do not answer within 2 minutes you will need to use `?register` again." }
            });
            await Task.Delay(500);
            SocketMessage response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
            if(response.Content.Length > 18)
            {
                await channel.SendMessageAsync("", false, new EmbedBuilder() {Title = "Invalid Name!", Color = Color.Red, Description = "Please provide a valid showdown name." });
                goto Restart;
            }
            player.ShowdownName = response.Content;

            Team:
            await channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "☆ BulbaLeague Signups ☆",
                Description = $"Okay, {player.ShowdownName}! Please paste your team below. Type `howto` if you don't know where to find your team data.",
                Footer = new EmbedFooterBuilder() { Text = "If you do not answer within 2 minutes you will need to use `?register` again." }
            });
            await Task.Delay(500);
            response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
            player.Team = response.Content;

            if (response.Content.ToLower().StartsWith("howto"))
            {
                await channel.SendMessageAsync("TODO: Add / Link to explanation");
                goto Team;
            }

            await channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "☆ BulbaLeague Signups ☆",
                Description = $"Is this data correct?\n" +
                $"**Discord Username:** {player.DiscordName}\n" +
                $"**Showdown Username:** {player.ShowdownName}\n**Team:**"
            });
            await Task.Delay(500);
            await channel.SendMessageAsync($"```{player.Team}```");
            await Task.Delay(500);
            await channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "☆ BulbaLeague Signups ☆",
                Description = $"Please respond with Yes or No",
                Footer = new EmbedFooterBuilder() { Text = "If you do not answer within 2 minutes you will need to use `?register` again." }
            });
            await Task.Delay(500);
            Confirm:
            response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));

            if (response.Content.ToLower() == "yes")
            {
                await channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Gold,
                    Title = "☆ BulbaLeague Signups ☆",
                    Description = $"Thanks for signing up {Context.User.Username}!" //add more info here
                });

                if (!(await AddPlayerToDatabase(player, tourney)))
                    await channel.SendMessageAsync("An error occured. Please contact a moderator.");
            }
            else if (response.Content.ToLower() == "no")
                goto Restart;
            else
                goto Confirm;
        }

        public async Task<bool> AddPlayerToDatabase(Player player, Tourney tourney)
        {
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            bool ok;
            try
            {
                await conn.OpenAsync();

                string cmdString = $"INSERT INTO participants (tid, uid, discordusername, showdownusername, team) VALUES ({tourney.ID}, '{player.Id}', '{player.DiscordName}', @ShowdownUsername, @Team)";
                MySqlCommand cmd = new MySqlCommand(cmdString, conn);
                cmd.Parameters.Add("@ShowdownUsername", MySqlDbType.VarChar).Value = player.ShowdownName;
                cmd.Parameters.Add("@Team", MySqlDbType.VarChar).Value = player.Team;

                await cmd.ExecuteNonQueryAsync();
                ok = true;
                
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "AddPlayerToDatabase", LogSeverity.Error);
                ok = false;
            }
            finally
            {
                await conn.CloseAsync();
            }

            return ok;
        }
    }
}

