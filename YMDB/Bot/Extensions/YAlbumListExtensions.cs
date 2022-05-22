using System.Collections.Generic;
using System.Linq;

using Yandex.Music.Api.Models.Album;

namespace YMDB.Bot.Utils
{
    public static class YAlbumListExtensions
    {
        public static string toString(this List<YAlbum> listAlbums)
        {
            var albums = "" + listAlbums.First().Title;
            foreach (var artist in listAlbums.GetRange(1, listAlbums.Count-1))
            {
                albums += ", " + artist.Title;
            }

            return albums;
        }
    }
}