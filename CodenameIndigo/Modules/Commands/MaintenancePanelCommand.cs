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
            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            Tourney tourney = await DatabaseHelper.GetLatestTourneyAsync();

            EmbedBuilder builder = new EmbedBuilder() { Title = "Mod CP", Color = Color.DarkGrey, Footer = new EmbedFooterBuilder() { Text = "If you do not answer within 2 minutes you will need to use `?modcp` again." } };
            builder.Description = $"Hey there {Context.User.Username}! You're currently editing tourney #{tourney.ID} - {tourney.Name}\nPlease choose one of these options by sending their number to me.\n" +
                $"1. Show/Change Signup Date\n" +
                $"2. Show/Change Min player amount\n" +
                $"3. Show/Change max player amount\n" +
                $"4. Remove a user from the Tournament.\n" +
                $"5. Start New Tourney - **Warning Destructive action**\n" +
                $"6. Close Menu";
            Restart:
            await channel.SendMessageAsync("", false, builder.Build());

            SocketMessage response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));

            if (Int32.TryParse(response.Content, out int choice))
            {
                switch (choice)
                {
                    case 1:
                        goto SignupDate;
                    case 2:
                        goto MinPlayer;
                    case 3:
                        goto MaxPlayer;
                    case 4:
                        goto RemoveUser;
                    case 5:
                        goto ResetTourney;
                    case 6:
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

                MySqlCommand cmd = new MySqlCommand("SELECT `regend` FROM `tournaments` WHERE `tid` = " + tourney.ID, conn);

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
                await channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Registration End Date",
                    Description = $"The current registration end date is set to: {offset.ToString("dd-MM-yyyy")}\n" +
                    $"If you would like to alter it, please send `edit`. Send anything else to return to the main menu."
                });

                response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
                if (response.Content.ToLower().StartsWith("edit"))
                {
                    Date_Time:
                    await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Registration End Date", Description = "Please provide the end date in the following format: DD-MM-YYYY" });

                    response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
                    try
                    {
                        DateTimeOffset date = Convert.ToDateTime(response.Content);
                        date = date.AddHours(date.Offset.Hours);
                        if (date.DayOfYear < DateTime.Now.DayOfYear || date.Year < DateTime.Now.Year)
                        {
                            await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Wrong Input!", Color = Color.Red, Description = "The date needs to be today or later!" });
                            goto Date_Time;
                        }
                        try
                        {
                            await conn.OpenAsync();

                            cmd = new MySqlCommand($"UPDATE `tournaments` SET `regend` = {date.ToUnixTimeSeconds()}  WHERE `tid` = " + tourney.ID, conn);
                            await cmd.ExecuteNonQueryAsync();
                            await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Edit Succesfull!", Color = Color.Green, Description = "Successfully changed the date. Returning to menu." });
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

            #region MinPlayer
            MinPlayer:
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT `minplayers` FROM `tournaments` WHERE `tid` = " + tourney.ID, conn);

                int min_players = 0;

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        min_players = reader.GetInt16(0);
                    }
                }
                await conn.CloseAsync();
                await channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Registration Min Players",
                    Description = $"The current minimum amount of players is {min_players} (minimum minimum is 2)\n" +
                    $"If you would like to alter it, please send `edit`. Send anything else to return to the main menu."
                });

                response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
                if (response.Content.ToLower().StartsWith("edit"))
                {
                    Min_Players:
                    await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Registration Min Players", Description = "What will be the minimum amount of players? (min 2)" });

                    response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
                    try
                    {
                        if (Int32.TryParse(response.Content, out int input))
                        {
                            if (input >= 2)
                            {
                                try
                                {
                                    await conn.OpenAsync();
                                    cmd = new MySqlCommand($"UPDATE `tournaments` SET `minplayers` = {input}  WHERE `tid` = " + tourney.ID, conn);
                                    await cmd.ExecuteNonQueryAsync();
                                    await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Edit Succesfull!", Color = Color.Green, Description = "Successfully changed the min player amount. Returning to menu." });
                                    await Task.Delay(1000);
                                }
                                catch (Exception e)
                                {
                                    await Program.Log(e.ToString(), "MinPlayer SQL", LogSeverity.Error);
                                    await channel.SendMessageAsync("An error occured. Please contact an administrator.");
                                }
                            }
                            else
                            {
                                await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Wrong Input!", Color = Color.Red, Description = "Please input a number higher than 1." });
                                goto Min_Players;
                            }
                        }
                        else
                        {
                            await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Wrong Input!", Color = Color.Red, Description = "Please input just a number." });
                            goto Min_Players;
                        }
                    }
                    catch (Exception e)
                    {
                        await Program.Log(e.ToString(), "TryParse Convesion", LogSeverity.Warning);
                    }
                }
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "MinPlayers SQL", LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            goto Restart;
            #endregion

            #region MaxPlayer
            MaxPlayer:
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT `maxplayers` FROM `tournaments` WHERE `tid` = " + tourney.ID, conn);

                int max_players = 0;

                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        max_players = reader.GetInt16(0);
                    }
                }
                await conn.CloseAsync();
                await channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Registration Max Players",
                    Description = $"The current maximum amount of players is {max_players} (minimum maximum is 2)\n" +
                    $"If you would like to alter it, please send `edit`. Send anything else to return to the main menu."
                });

                response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
                if (response.Content.ToLower().StartsWith("edit"))
                {
                    Min_Players:
                    await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Registration Max Players", Description = "What will be the maximum amount of players? (min 2)" });

                    response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
                    try
                    {
                        if (Int32.TryParse(response.Content, out int input))
                        {
                            if (input >= 2)
                            {
                                try
                                {
                                    await conn.OpenAsync();
                                    cmd = new MySqlCommand($"UPDATE `tournaments` SET `maxplayers` = {input}  WHERE `tid` = " + tourney.ID, conn);
                                    await cmd.ExecuteNonQueryAsync();
                                    await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Edit Succesfull!", Color = Color.Green, Description = "Successfully changed the max player amount. Returning to menu." });
                                    await Task.Delay(1000);
                                }
                                catch (Exception e)
                                {
                                    await Program.Log(e.ToString(), "MaxPlayer SQL", LogSeverity.Error);
                                    await channel.SendMessageAsync("An error occured. Please contact an administrator.");
                                }
                            }
                            else
                            {
                                await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Wrong Input!", Color = Color.Red, Description = "Please input a number higher than 1." });
                                goto Min_Players;
                            }
                        }
                        else
                        {
                            await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Wrong Input!", Color = Color.Red, Description = "Please input just a number." });
                            goto Min_Players;
                        }
                    }
                    catch (Exception e)
                    {
                        await Program.Log(e.ToString(), "TryParse Convesion", LogSeverity.Warning);
                    }
                }
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "MaxPlayers SQL", LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
            goto Restart;
            #endregion

            #region RemoveUser
            RemoveUser:
            await channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Title = "Remove User from Tournament",
                Color = Color.DarkOrange,
                Description = "Please provide the Discord ID or Discord Username (with discriminator) of the user you're trying to kick."
            });
            response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));

            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand("SELECT * FROM `participants` WHERE `uid` = @id OR `discordusername` = @name", conn);

                cmd.Parameters.Add("@id", MySqlDbType.Int64).Value = long.TryParse(response.Content, out long value) ? value : 000000000000000000;
                cmd.Parameters.Add("@name", MySqlDbType.VarChar).Value = response.Content;

                Player player = new Player();
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        player.Id = reader.GetUInt64(2);
                        player.DiscordName = reader.GetString(3);
                        player.ShowdownName = reader.GetString(4);
                    }
                }
                if (string.IsNullOrEmpty(player.DiscordName))
                {
                    await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "User not Found", Color = Color.Red, Description = "User not found. Returning to main menu." });
                    await Task.Delay(500);
                    goto Restart;
                }
                await conn.CloseAsync();
                await channel.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Title = "Remove User",
                    Color = Color.DarkOrange,
                    Description = $"Is this the user you're looking for?\n" +
                    $"**Discord ID:** {player.Id}\n" +
                    $"**Discord Name:** {player.DiscordName}\n" +
                    $"**Showdown Name:** {player.ShowdownName}\n" +
                    $"Please respond with yes/no"
                });
                response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
                if (response.Content.ToLower().Equals("yes"))
                {
                    try
                    {
                        await conn.OpenAsync();
                        cmd = new MySqlCommand("DELETE FROM `participants` WHERE `uid` = " + player.Id, conn);
                        await cmd.ExecuteNonQueryAsync();
                        await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Success!", Color = Color.LightOrange, Description = $"Successfully kicked {player.DiscordName} from the game.\nReturning to main menu." });
                        await Task.Delay(500);
                    }
                    catch (Exception e)
                    {
                        await Program.Log(e.ToString(), "RemoveUser => Deletion", LogSeverity.Error);
                    }
                }
                else
                {
                    await channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Cancelled.", Color = Color.Red, Description = "Process Cancelled. Returning to main menu." });
                    await Task.Delay(500);
                    goto Restart;
                }
            }
            catch (Exception e)
            {
                await Program.Log(e.ToString(), "RemoveUser => Lookup", LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }


            goto Restart;
            #endregion

            #region ResetTourney
            ResetTourney:
            await channel.SendMessageAsync("", false, new EmbedBuilder()
            {
                Title = "**Tournament Reset**",
                Color = Color.DarkRed,
                Description = "Do you wish to reset the tournament? (yes/no)\n" +
                "This option is **destructive** and will reset **all** values to their default, and remove **everyone** from the current list of players."
            });

            response = await NextMessageAsync(new EnsureChannelCriterion(channel.Id), TimeSpan.FromMinutes(2));
            if (response.Content.Equals("yes"))
            {
                try
                {
                    await conn.OpenAsync();

                    MySqlCommand resetSettings = new MySqlCommand("UPDATE `setup` SET `end_date`=0,`min_players`=2,`max_players`=-1 WHERE `tourney_id` = 1", conn);
                    MySqlCommand truncateUsers = new MySqlCommand("TRUNCATE TABLE users", conn);

                    await resetSettings.ExecuteNonQueryAsync();
                    await truncateUsers.ExecuteNonQueryAsync();

                    await channel.SendMessageAsync("", false, new EmbedBuilder()
                    {
                        Title = "Tournament Reset Complete",
                        Color = Color.DarkRed,
                        Description = "Tournament has been reset."
                    });
                    await Task.Delay(500);
                }
                catch (Exception e)
                {
                    await Program.Log(e.ToString(), "ResetTourney SQL", LogSeverity.Error);
                    await channel.SendMessageAsync("An error occured. Please contact an admin");
                }
                finally
                {
                    await conn.CloseAsync();
                }
            }

            goto Restart;
            #endregion
        }
    }
}
