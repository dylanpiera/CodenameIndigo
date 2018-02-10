using Discord.Addons.Interactive;
using Discord.Commands;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace CodenameIndigo.Modules
{
    public class ConnectionTest : InteractiveBase
    {
        [Command("unix")]
        public async Task Unix()
        {
            await Context.Channel.SendMessageAsync(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }

        [Command("conntest")]
        public async Task ConnTest()
        {
            MySqlConnection conn = GetClosedConnection();
            try
            {
                await conn.OpenAsync();
                await Context.Channel.SendMessageAsync("Success.");
            }
            catch (Exception e)
            {
                await Context.Channel.SendMessageAsync(e.ToString().Length > 1999 ? e.ToString().Remove(1999) : e.ToString());
                await Program.Log(e.ToString(), "Connection Test", Discord.LogSeverity.Error);
            }
            finally
            {
                await conn.CloseAsync();
            }
        }
        
        public static MySqlConnection GetClosedConnection()
        {
            string connStr = $"Server={Sneaky.DatabaseUrl};Uid={Sneaky.User};Database=bulbaleague;port=3306;Password={Sneaky.Password};SslMode=none;CharSet=utf8mb4";
            MySqlConnection conn = new MySqlConnection(connStr) { };
            return conn;
        }
    }
}