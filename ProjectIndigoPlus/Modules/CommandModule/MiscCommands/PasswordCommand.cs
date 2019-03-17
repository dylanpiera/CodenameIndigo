using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MySql.Data.MySqlClient;
using ProjectIndigoPlus;
using System;
using System.Threading.Tasks;

namespace ProjectIndigoPlus.Modules.Commands
{
    internal class Password
    {
        [Command("getpassword"), Aliases(new[] { "getp", "password" }), Hidden()]
        public async Task GetPassword(CommandContext context)
        {
            string connStr = $"Server={Bot._config.DbServer};Uid={Bot._config.DbUser};Database=amasenior;port=3306;Password={Bot._config.DbPass};SslMode=none;CharSet=utf8mb4";
            MySqlConnection conn = new MySqlConnection(connStr) { };
            try
            {
                await conn.OpenAsync();

                MySqlCommand cmd = new MySqlCommand($"SELECT `password` FROM `users` WHERE `name` = \"{context.Member?.Nickname ?? context.User.Username}\" OR `name` = \"{context.User.Username}\"", conn);

                string pass = null;
                using (MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                         pass = reader.GetString("password");
                    }
                }
				
                if (string.IsNullOrEmpty(pass))
                {
                    if (context.Member != null)
                    {
                        await context.Member.SendMessageAsync("Your password could not be found. Please contact either SoaringDylan#0380 or Saphir#0001");
                    }
                    else
                    {
                        await context.RespondAsync("Your password could not be found. Please contact either SoaringDylan#0380 or Saphir#0001");
                    }
                }
                else
                {
                    if (context.Member != null)
                    {
                        await context.Member.SendMessageAsync("", false, new DiscordEmbedBuilder() { Description = $"Your password is:\n\n`{pass}`\n\nDo not share this with others.", Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = "If you require your password to be reset please contact either SoaringDylan#0380 or Saphir#0001" } });
                    }
                    else
                    {
                        await context.RespondAsync("", false, new DiscordEmbedBuilder() { Description = $"Your password is:\n\n`{pass}`\n\nDo not share this with others.", Footer = new DiscordEmbedBuilder.EmbedFooter() { Text = "If you require your password to be reset please contact either SoaringDylan#0380 or Saphir#0001" } });
                    }
                }
            }
            catch (Exception e)
            {
            	
                Bot.DebugLogger.LogMessage(DSharpPlus.LogLevel.Critical, "Password", e.ToString(), DateTime.Now);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
    }
}