using System;
using System.Collections.Generic;
using System.Linq;
using Yandex.Music.Api.Models.Artist;

namespace YMDB.Bot.Utils
{
    public static class YArtistListExtensions
    {
        public static string toString(this List<YArtist> listArtists)
        {
            string artists = string.Join(" , ", listArtists.Select(p => p.Name) );
            if (artists == "") artists = "Нет артиста";
            return artists;
        }
    }
}