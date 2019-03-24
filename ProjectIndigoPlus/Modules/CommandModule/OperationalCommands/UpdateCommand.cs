using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus.Modules.HelperModule;
using ProjectIndigoPlus.Modules.ModelModule;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class Update
    {
        private const ulong ANNOUNCEMENT_CHANNEL = 409419573322973194;

        [Command("tourneyupdate"),
            Aliases(new[] { "updatetourney", "tupdate" }),
            Description("Updates the state of the current tournament")]
        public async Task Command(CommandContext context, [Description("Arguments to the command")] params string[] args)
        {
            Dictionary<ArgType, string> arguments = CommandArgs.ReadArgs(args);

            bool forced = false;

            foreach (ArgType key in arguments.Keys)
            {
                switch (key)
                {
                    case ArgType.Force:
                        forced = true;
                        continue;
                }
            }

            MySqlConnection conn = DatabaseHelper.GetClosedConnection();
            TourneyModel tourney = await conn.GetLatestTourneyAsync();

            switch (tourney.State)
            {
                //pre-signup
                case 0:
                    if (forced || tourney.RegStart <= DateTime.Now)
                    {
                        try
                        {
                            await conn.OpenAsync();
                            MySqlCommand cmd = new MySqlCommand($"UPDATE `tournaments` SET `state` = 1 WHERE `tid` = {tourney.Tid}", conn);

                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch (Exception e)
                        {
                            Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "TUpdate", e.ToString(), DateTime.Now);
                            await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                            {
                                Color = DiscordColor.Red,
                                Title = "Tourney Updater",
                                Description = $"An error occured while processing your request. Please contact a staff member."
                            });
                            return;
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }

                        await (await context.Client.GetGuildAsync(202737349996576769)).GetChannel(ANNOUNCEMENT_CHANNEL).SendMessageAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = Bot._config.Color,
                            Title = $"Signups for {tourney.Name} have opened!",
                            Description = $"Signups have now opened! Join with the `?join` command!\n\nFor more info and rules visit: https://bulbaleague.bmgs.site/rules",
                            Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = $"The registration closes at: {tourney.RegEnd.ToString("ddd, MMM d yyyy HH:mm")} UTC +0" },
                            Timestamp = DateTime.Now
                        });
                        await Command(context, "");
                        return;
                    }
                    else
                    {
                        await context.RespondAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Orange,
                            Title = "Registrations closed - No update available",
                            Description = $"the tournament is currently still in pre-signups. The registration start date is set to: {tourney.RegStart.ToString()} :warning:"
                        });
                    }
                    break;
                //signups
                case 1:
                    if (forced || tourney.RegEnd <= DateTime.Now)
                    {
                        try
                        {
                            await conn.OpenAsync();
                            MySqlCommand cmd = new MySqlCommand($"UPDATE `tournaments` SET `state` = 2 WHERE `tid` = {tourney.Tid}", conn);

                            await cmd.ExecuteNonQueryAsync();
                        }
                        catch (Exception e)
                        {
                            Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "TUpdate", e.ToString(), DateTime.Now);
                            await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                            {
                                Color = DiscordColor.Red,
                                Title = "Tourney Updater",
                                Description = $"An error occured while processing your request. Please contact a staff member."
                            });
                            return;
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }

                        await (await context.Client.GetGuildAsync(202737349996576769)).GetChannel(ANNOUNCEMENT_CHANNEL).SendMessageAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Red,
                            Title = $"Signups for {tourney.Name} have closed!",
                            Description = $"Signups have now Closed! Once our staff has double checked everyone's team, the tournament will start.",
                            Timestamp = DateTime.Now
                        });
                        await Command(context, "");
                        return;
                    }
                    else
                    {
                        await context.RespondAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Orange,
                            Title = "Registrations ongoing - No update available",
                            Description = $"the tournament is currently still in signups. Players can signup till: {tourney.RegEnd.ToString()} :warning:"
                        });
                    }
                    break;
                //confirmation
                case 2:
                    List<ulong> users = new List<ulong>();
                    try
                    {
                        await conn.OpenAsync();
                        MySqlCommand cmd = new MySqlCommand($"SELECT `uid`, `checked` FROM `teams` WHERE `tid` = " + tourney.Tid, conn);

                        using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                if (!forced)
                                {
                                    if (await reader.IsDBNullAsync(reader.GetOrdinal("checked")))
                                    {
                                        await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                                        {
                                            Color = DiscordColor.Orange,
                                            Title = "Confirmation Required - No update available",
                                            Description = $"There are still some teams that need to be checked before the tourney can start."
                                        });
                                        return;
                                    }
                                }
                                users.Add(reader.GetUInt64("uid"));
                            }
                        }

                        cmd = new MySqlCommand($"UPDATE `tournaments` SET `state` = 3 WHERE `tid` = {tourney.Tid}", conn);

                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception e)
                    {
                        Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "TUpdate", e.ToString(), DateTime.Now);
                        await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Red,
                            Title = "Tourney Updater",
                            Description = $"An error occured while processing your request. Please contact a staff member."
                        });
                        return;
                    }
                    finally
                    {
                        await conn.CloseAsync();
                    }

                    await (await context.Client.GetGuildAsync(202737349996576769)).GetChannel(ANNOUNCEMENT_CHANNEL).SendMessageAsync("", false, new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Orange,
                        Title = $"{tourney.Name} is starting!",
                        Description = $"Signups have closed, teams have been checked. Randomizing brackets...",
                        Timestamp = DateTime.Now
                    });
                    await Task.Delay(100);
                    await (await context.Client.GetGuildAsync(202737349996576769)).GetChannel(ANNOUNCEMENT_CHANNEL).TriggerTypingAsync();

                    List<Tuple<ulong, ulong>> battles = new List<Tuple<ulong, ulong>>();

                    Random rand = new Random();
                    if (users.Count % 2 == 1)
                    {
                        int r = rand.Next(users.Count);
                        battles.Add(new Tuple<ulong, ulong>(users[r - 1], 0));
                        users.RemoveAt(r - 1);
                    }
                    try
                    {
                        users.Sort();
                        int n = users.Count;
                        while (n > 1)
                        {
                            n--;
                            int k = rand.Next(n + 1);
                            ulong value = users[k];
                            users[k] = users[n];
                            users[n] = value;
                        }
                        for (int i = 0; i < users.Count; i += 2)
                        {
                            battles.Add(new Tuple<ulong, ulong>(users[i], users[i + 1]));
                        }

                        try
                        {
                            await conn.OpenAsync();

                            foreach (Tuple<ulong, ulong> battle in battles)
                            {
                                string sql;
                                if (battle.Item2 == 0)
                                {
                                    sql = $"INSERT INTO `battles` (`tid`, `round`, `player1`, `winner`) VALUES ({tourney.Tid}, 1, {battle.Item1}, 0)";
                                }
                                else
                                {
                                    sql = $"INSERT INTO `battles` (`tid`, `round`, `player1`, `player2`, `winner`) VALUES ({tourney.Tid}, 1, {battle.Item1}, {battle.Item2}, 0)";
                                }
                                MySqlCommand cmd = new MySqlCommand(sql, conn);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        catch (Exception e)
                        {
                            Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "TUpdate", e.ToString(), DateTime.Now);
                            await context.Channel.SendMessageAsync("", false, new DiscordEmbedBuilder()
                            {
                                Color = DiscordColor.Red,
                                Title = "Tourney Updater",
                                Description = $"An error occured while processing your request. Please contact a staff member."
                            });
                            return;
                        }
                        finally
                        {
                            await conn.CloseAsync();
                        }

                        
                        await (await context.Client.GetGuildAsync(202737349996576769)).GetChannel(ANNOUNCEMENT_CHANNEL).SendMessageAsync("", false, await Battle.BuildBattleMessage(new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Orange,
                            Title = $"{tourney.Name} brackets!",
                            Timestamp = DateTime.Now
                        }));
                    }
                    catch (Exception e)
                    {
                        Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "TUpdate", e.ToString(), DateTime.Now);
                    }

                    break;
                //rounds
                case short i when i >= 3 && i <= 50:

                    break;
                //finished
                case 100:
                    break;
            }
            await Task.Delay(150);
            await context.RespondAsync("", false, new DiscordEmbedBuilder()
            {
                Color = Bot._config.Color,
                Title = "Update Complete",
                Description = "Finished updating the tournament state. :white_check_mark:"
            });
        }
    }
}
