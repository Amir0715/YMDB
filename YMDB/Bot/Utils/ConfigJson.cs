using Newtonsoft.Json;

namespace YMDB.Bot.Utils
{
    public struct ConfigJson
    {
        [JsonProperty("discordToken")]
        public string DiscordToken { get; private set; }
        
        [JsonProperty("yandexToken")]
        public string YandexToken { get; private set; }

        [JsonProperty("prefix")]
        public string CommandPrefix { get; private set; }
        
        [JsonProperty("login")]
        public string Login { get; private set; }
        
        [JsonProperty("password")]
        public string Password { get; private set; }

        [JsonProperty("downloadPath")]
        public string DownloadPath { get; private set; }
    }
}
