using System.Text.RegularExpressions;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;
using YMDB.Bot.Yandex;

namespace YMDB.Bot.Utils
{
    public static class UrlUtils
    {
        public static YTrack GetTrack(string url)
        {
            // https://music.yandex.ru/album/9370221/track/60586649

            var trackid = url.Split('/')[^1];
            var track = YMDownloader.GetInstance().Ymc.GetTrack(trackid);
            return track;
        }

        public static YPlaylist GetPlaylist(string url)
        {
            // https://music.yandex.ru/users/kamolov.amir2000/playlists/1001
            
            var ownername = url.Split('/')[^3];
            var playlistid = url.Split('/')[^1];
            var playlist = YMDownloader.GetInstance().Ymc.GetPlaylist(ownername, playlistid);
            return playlist;
        }

        public static YAlbum GetAlbum(string url)
        {
            // https://music.yandex.ru/album/2218476
            
            var albumid = url.Split('/')[^1];
            var album = YMDownloader.GetInstance().Ymc.GetAlbum(albumid);
            return album;
        }

        public static YArtistBriefInfo GetArtistBriefInfo(string url)
        {
            // https://music.yandex.ru/artist/8036520

            var artistBriefInfoid = url.Split('/')[^1];
            var artistBriefInfo = YMDownloader.GetInstance().Ymc.GetArtist(artistBriefInfoid);
            return artistBriefInfo;
        }

        public static YArtist GetArtist(string url)
        {
            // https://music.yandex.ru/artist/8036520

            var artist = GetArtistBriefInfo(url).Artist;
            return artist;
        }

        public static TypeOfUrl GetTypeOfUrl(string url)
        {
            var regex = new Regex(@"track/(\d*)$");
            if (regex.IsMatch(url))
                return TypeOfUrl.TRACK;

            regex = new Regex(@"album/(\d*)$");
            if (regex.IsMatch(url))
                return TypeOfUrl.ALBUM;

            regex = new Regex(@"users/(.*)/playlists/(\d*)$");
            if (regex.IsMatch(url))
                return TypeOfUrl.PLAYLIST;

            regex = new Regex(@"artist/(\d*)$");
            if (regex.IsMatch(url))
                return TypeOfUrl.ARTIST;
            
            return TypeOfUrl.NONE;
        }
        
        public enum TypeOfUrl
        {
            NONE = 0,
            TRACK = 1,
            ALBUM = 2,
            PLAYLIST = 3,
            ARTIST = 4
        }
    }
}