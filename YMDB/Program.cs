using System;
using System.IO;
using System.Text;

using DSharpPlus;

using Newtonsoft.Json;

using YMDB.Bot.Utils;

namespace YMDB
{
    public class Program
    {
        public DiscordClient Discord { get; set; }

        public static void Main(string[] args)
        {
            var botConfigPath = "";
            if (args.Length == 0)
            {
                botConfigPath = "Config/BotConfig.json";
            }
            else if (args.Length == 1)
            {
                botConfigPath = args[0];
                Console.WriteLine(args[0]);
            }

            var json = "";
            using (var fs = File.OpenRead(botConfigPath))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
            {
                json = sr.ReadToEnd();
            }

            var cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var bot = new Bot.Bot(cfgjson);
            bot.StartAsync().GetAwaiter().GetResult();
        }
    }
}
