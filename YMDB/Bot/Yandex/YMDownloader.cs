using System.IO;
using Yandex.Music.Api.Models.Track;
using Yandex.Music.Client;
using Yandex.Music.Client.Extensions;
using YMDB.Bot.Utils;

namespace YMDB.Bot.Yandex
{
    public class YMDownloader
    {
        public YandexMusicClient Ymc { get; private set; }
        private static YMDownloader Instance;
        
        private YMDownloader(string login, string password)
        {
            Ymc = new YandexMusicClient();
            Ymc.Authorize(login, password);
        }

        public static YMDownloader GetInstance(string login = null, string password = null)
        {
            return Instance ??= new YMDownloader(login, password);
        }

        public YTrack DownloadTrack(string url)
        {
            // TODO: проверка на наличие файла
            var track = UrlUtils.GetTrack(url);
            var path = $"Data/{track.Id}.mp3";
            if (!File.Exists(path))
            {
                track.Save(path);
            }
            return track;
        }
        
    }
}