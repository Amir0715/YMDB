using System.Collections.Generic;
using System.Linq;
using Yandex.Music.Api.Models.Artist;

namespace YMDB.Bot.Utils
{
    public static class YArtistListExtensions
    {
        public static string toString(this List<YArtist> listArtists)
        {
            var artists = "" + listArtists.First().Name;
            foreach (var artist in listArtists.GetRange(1, listArtists.Count-1))
            {
                artists += ", " + artist;
            }

            return artists;
        }
    }
}