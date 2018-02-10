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

namespace CodenameIndigo.Modules.Commands
{
    public class MaintenancePanelCommand : InteractiveBase
    {
        [Command("modcp", RunMode = RunMode.Async), MaintenancePrecon()]
        public async Task ModCP()
        {
            try
            {
                await ReplyAndDeleteAsync($"Hey {Context.User.Mention}! I've send you a PM with the info!", false, null, TimeSpan.FromSeconds(15));
                await Context.Message.DeleteAsync();
            }
            catch { }

            IDMChannel channel = await Context.User.GetOrCreateDMChannelAsync();
            MySqlConnection conn = ConnectionTest.GetClosedConnection();

            EmbedBuilder builder = new EmbedBuilder() {Title = "Mod CP", Color = Color.DarkGrey, Footer = new EmbedFooterBuilder() { Text = "If you do not answer within 2 minutes you will need to use `?modcp` again." } };
            builder.Description = $"Hey there {Context.User.Username}! Please choose one of these options by sending their number to me.\n" +
                $"1. Show/Change Signup Date\n" +
                $"2. Show/Change Min player amount\n" +
                $"3. Show/Change max player amount\n" +
                $"4. Start New Tourney - **Warning Destructive action**\n" +
                $"5. Close Menu";
            Restart:
            await channel.SendMessageAsync("",false, builder.Build());
            await Task.Delay(500);

            SocketMessage response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));

            if(Int32.TryParse(response.Content, out int choice))
            {
                switch(choice)
                {
                    case 1:
                        goto SignupDate;
                    case 2:
                        goto MinPlayer;
                    case 3:
                        goto MaxPlayer;
                    case 4:
                        goto ResetTourney;
                    case 5:
                        return;
                }
            }
            else
            {
                await channel.SendMessageAsync("Please choose one of the options.");
                goto Restart; 
            }
            #region SignupDate
            SignupDate:
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT `end_date` FROM `setup` WHERE `tourney_id` = 1", conn);

                long UNIXTime = 0;

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        UNIXTime = reader.GetInt64(0);
                    }
                }

                DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(UNIXTime);
                await conn.CloseAsync();
                await channel.SendMessageAsync("", false, new EmbedBuilder() {Title = "Registration End Date", Description = $"The current registration end date is set to: {offset.ToString("dd-MM-yyyy")}\n" +
                    $"If you would like to alter it, please send `edit`. Send anything else to return to the main menu." });
                await Task.Delay(500);

                response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
                if(response.Content.ToLower().StartsWith("edit"))
                {
                    Date_Time:
                    await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Registration End Date", Description = "Please provide the end date in the following format: DD-MM-YYYY" });
                    await Task.Delay(500);

                    response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
                    try
                    {
                        DateTimeOffset date = Convert.ToDateTime(response.Content);
                        date = date.AddHours(date.Offset.Hours);
                        if(date.DayOfYear < DateTime.Now.DayOfYear || date.Year < DateTime.Now.Year)
                        {
                            await channel.SendMessageAsync("", false, new EmbedBuilder() {Title = "Wrong Input!", Color = Color.Red, Description = "The date needs to be today or later!" });
                            goto Date_Time;
                        }
                        try
                        {
                            await conn.OpenAsync();

                            cmd = new MySqlCommand($"UPDATE `setup` SET `end_date` = {date.ToUnixTimeSeconds()}  WHERE `tourney_id` = 1", conn);
                            await cmd.ExecuteNonQueryAsync();
                            await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Edit Succesfull!", Color = Color.Green, Description = "Successfully changed the date. Returning to menu."});
                            await Task.Delay(1000);
                        }
                        catch (Exception e)
                        {
                            await Program.Log(e.ToString(), "SignupDate SQL", LogSeverity.Error);
                            await channel.SendMessageAsync("An error occured. Please contact an administrator.");
                        }
                    }
                    catch (Exception e)
                    {
                        await Program.Log(e.ToString(), "DateTime Convesion", LogSeverity.Warning);
                        goto Date_Time;
                    }
                }   
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "SignupDate SQL", LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            goto Restart;
            #endregion

            MinPlayer:

            goto Restart;

            MaxPlayer:

            goto Restart;

            ResetTourney:

            goto Restart;
        }
    }
}
