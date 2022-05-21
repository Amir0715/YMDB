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
        private readonly string _downloadDir;
        
        private YMDownloader(string login, string password, string downloadDir)
        {
            _downloadDir = downloadDir;
            if (!Directory.Exists(_downloadDir))
                Directory.CreateDirectory(_downloadDir);
            Ymc = new YandexMusicClient();
            Ymc.Authorize(login, password);
        }

        public static YMDownloader GetInstance(string login = null, string password = null, string downloadPath = null)
        {
            return Instance ??= new YMDownloader(login, password, downloadPath);
        }

        public string DownloadTrack(string url)
        {
            // TODO: проверка на наличие файла
            var track = UrlUtils.GetTrack(url);
            return DownloadTrack(track);
        }
        
        public string DownloadTrack(YTrack track)
        {
            // TODO: проверка на наличие файла
            var trackPath = $@"{track.Id}.mp3";
            var path = Path.Combine(_downloadDir, trackPath);
            
            if (!File.Exists(path))
                track.Save(path);
            
            return path;
        }
    }
}