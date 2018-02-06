﻿using CodenameIndigo.Modules.Preconditions;
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
        [Command("lookup", RunMode = RunMode.Async)]
        public async Task LookupCommand(IUser mention)
        {
            MySqlConnection conn = ConnectionTest.GetClosedConnection();
            try
            {
                await conn.OpenAsync();

                string cmdString = $"SELECT Team FROM users WHERE UUID = {mention.Id}";
                MySqlCommand cmd = new MySqlCommand(cmdString, conn);

                string team = "";
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        team = reader.GetString(reader.GetOrdinal("Team"));
                    }
                }
                if (string.IsNullOrEmpty(team))
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "User not Found", Description = "It appears that user isn't registered for the tournament.", Color = Color.Red });
                else
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder() { Title = $"{mention.Username}'s Team", Description = team });
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
    }
}
