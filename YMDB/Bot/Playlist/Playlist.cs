using System;
using System.Collections.Generic;
using System.Linq;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;
using YMDB.Bot.Utils;

/*
TODO: Добавить очередь песен и функционал для взаимодействия с ним.
    Методы взаимодействия:
        + Добавить песню в начало\конец\поиндексу\следущей.
        + Добавить песни из альбома\плейлиста в начало\конец\поиндексу\следущим.
        + Пропустить n-песен.
        Проигравоние песни из очереди по названию.
        + Перемещать очередь.
        + Зациклить песню\очередь\отрезок очереди.
        + Удалить из очереди песню\песни по индексу\индексам.
*/
namespace YMDB.Bot.Playlist
{
    public class Playlist
    {
        public List<YTrack> Tracks { get; private set; }

        public bool LoopedSong { get; set; }

        public Playlist()
        {
            Tracks = new List<YTrack>();
        }

        public int GetCount()
        {
            return Tracks.Count;
        }

        public void Insert(YTrack track, int index)
        {
            Tracks.Insert(index, track);
        }

        public void AddToBegin(YTrack track)
        {
            Tracks.Add(track);
        }
        
        public void AddToEnd(YTrack track)
        {
            Tracks.Insert(0, track);
        }

        public YTrack GetNext()
        {
            var track = Tracks[0];
            if (!LoopedSong)
                Tracks.RemoveAt(0);
            return track;
        }

        public void RemoveAt(int index)
        {
            Tracks.RemoveAt(index);
        }

        public YTrack Skip(int count)
        {
            Tracks.RemoveRange(0, count);
            return GetNext();
        }

        public void Shuffle()
        {
            var rand = new Random();
            Tracks = Tracks.OrderBy(x => rand.Next()).ToList();
        }

        public void AddToEnd(YPlaylist playlist)
        {
            var tracksContainers = playlist.Tracks;
            foreach (var trackContainer in tracksContainers)
            {
                AddToEnd(trackContainer.Track);
            }
        }

        public void AddToBegin(YPlaylist playlist)
        {
            var tracksContainers = playlist.Tracks;
            foreach (var trackContainer in Enumerable.Reverse(tracksContainers))
            {
                AddToBegin(trackContainer.Track);
            }
        }
        // https://music.yandex.ru/album/9683396
        public void AddToEnd(YAlbum album)
        {
            var volumes = album.Volumes;
            foreach (var track in volumes.SelectMany(volume => volume))
            {
                AddToEnd(track);
            }
        }

        public void AddToBegin(YAlbum album)
        {
            var volumes = album.Volumes;
            foreach (var volume in Enumerable.Reverse(volumes))
            {
                foreach (var track in Enumerable.Reverse(volume))
                {
                    AddToBegin(track);
                }
            }
        }
        
        public void Clear()
        {
            Tracks.Clear();
        }

        public override string ToString()
        {
            var result = "";
            var i = 0;
            foreach (var track in Tracks)
            {   
                result += $"{i++} `{track.Artists.toString()} - {track.Title}` \n";
            }
            return result;
        }
    }
}