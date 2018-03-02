using CodenameIndigo.Modules.Preconditions;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules.Commands
{
    public class TeamLookupCommand : InteractiveBase
    {
        [Command("lookup", RunMode = RunMode.Async), NotInSignupPrecon(0,Group = "a"), MaintenancePrecon(Group = "a")]
        public async Task LookupCommand(IUser mention)
        {
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            try
            {
                await conn.OpenAsync();

                string cmdString = $"SELECT team FROM participants WHERE tid = {(await DatabaseHelper.GetLatestTourneyAsync()).ID} AND uid = {mention.Id}";
                MySqlCommand cmd = new MySqlCommand(cmdString, conn);

                string team = "";
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        team = reader.GetString(reader.GetOrdinal("team"));
                    }
                }
                if (string.IsNullOrEmpty(team))
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Player not Found", Description = "It appears that user isn't registered for the tournament.", Color = Color.Red });
                else
                    await Context.Channel.SendMessageAsync($"**{mention.Username}'s Team**\n```{team}```");
            }
            catch (Exception e)
            {
                await Program.Log(new LogMessage(LogSeverity.Error, "Lookup Command", e.ToString()));
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Error", Description = "Please contact an administrator.", Color = Color.DarkRed});
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        [Command("lookup"), Priority(-1)]
        public async Task LookupCommand([Remainder] string s)
        {
            await Context.Channel.SendMessageAsync("",false,new EmbedBuilder() {Title = "User Not Found", Description = $"The user {s} doesn't exist.", Color = Color.Red });
        }
    }
}
