using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using DSharpPlus.Entities;
using System.IO;

namespace IndigoBot.Entities
{
    internal class Config
    {
        /// <summary>
        /// Your bot's token.
        /// </summary>
        [JsonProperty("token")]
        internal string Token = "Your token..";

        /// <summary>
        /// Your bot's prefix
        /// </summary>
        [JsonProperty("prefix")]
        internal string Prefix = "'";

        /// <summary>
        /// Your bot's database server adress
        /// </summary>
        [JsonProperty("DbServer")]
        internal string DbServer = "192.0.0.1";

        /// <summary>
        /// Your bot's database user
        /// </summary>
        [JsonProperty("DbUser")]
        internal string DbUser = "IndigoBot";

        /// <summary>
        /// Your database table
        /// </summary>
        [JsonProperty("DbName")]
        internal string DbName = "league";

        /// <summary>
        /// Your bot's database user's password
        /// </summary>
        [JsonProperty("DbPass")]
        internal string DbPass = "12345";

        /// <summary>
        /// Your favourite color.
        /// </summary>
        [JsonProperty("color")]
        private readonly string _color = "#7289DA";

        /// <summary>
        /// Your favourite color exposed as a DiscordColor object.
        /// </summary>
        internal DiscordColor Color => new DiscordColor(_color);

        /// <summary>
        /// Loads config from a JSON file.
        /// </summary>
        /// <param name="path">Path to your config file.</param>
        /// <returns></returns>
        public static Config LoadFromFile(string path)
        {
            using (var sr = new StreamReader(path))
            {
                return JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
            }
        }

        /// <summary>
        /// Saves config to a JSON file.
        /// </summary>
        /// <param name="path"></param>
        public void SaveToFile(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                sw.Write(JsonConvert.SerializeObject(this));
            }
        }
    }
}
