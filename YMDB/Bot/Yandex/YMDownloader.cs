using System.IO;

using Yandex.Music.Api.Models.Track;
using Yandex.Music.Client;
using Yandex.Music.Client.Extensions;

using YMDB.Bot.Utils;

namespace YMDB.Bot.Yandex
{
    public class YMDownloader
    {
        public YandexMusicClient Ymc { get; } = new();
        private static YMDownloader Instance;
        private string DownloadDir;
        
        private YMDownloader(string login, string password, string downloadDir)
        {
            SetDownloadDir(downloadDir);
            Ymc.Authorize(login, password);
        }
        
        private YMDownloader(string token, string downloadDir)
        {
            SetDownloadDir(downloadDir);
            Ymc.Authorize(token);
        }

        private void SetDownloadDir(string downloadDir)
        {
            DownloadDir = downloadDir;
            
            if (!Directory.Exists(DownloadDir))
                Directory.CreateDirectory(DownloadDir!);
        }

        public static YMDownloader GetInstance(string login, string password, string downloadPath)
        {
            return Instance ??= new YMDownloader(login, password, downloadPath);
        }
        
        public static YMDownloader GetInstance(string token, string downloadPath)
        {
            return Instance ??= new YMDownloader(token, downloadPath);
        }
        
        public static YMDownloader GetInstance()
        {
            return Instance;
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
            var path = Path.Combine(DownloadDir, trackPath);
            
            if (!File.Exists(path))
                track.Save(path);
            
            return path;
        }
    }
}
