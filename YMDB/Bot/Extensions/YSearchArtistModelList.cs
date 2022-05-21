using System.Collections.Generic;
using System.Linq;

using Yandex.Music.Api.Models.Search.Artist;

namespace YMDB.Bot.Extensions
{
    public static class YSearchArtistModelList
    {
        public static string toString(this List<YSearchArtist> listArtists)
        {
            string artists = string.Join(" , ", listArtists.Select(p => p.Name) );
            if (artists == "") artists = "Нет артиста";
            return artists;
        }
    }
}