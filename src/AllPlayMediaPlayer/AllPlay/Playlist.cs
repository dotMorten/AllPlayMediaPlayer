using net.allplay.MediaPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPlayMediaPlayer.AllPlay
{
    public class Playlist
    {
        public Playlist()
        {

        }
        public void UpdatePlaylist(IEnumerable<MediaItem> playlist)
        {
            Items = new List<MediaItem>(playlist);
            currentItemIndex = 0;
            CurrentItemChanged?.Invoke(this, (CurrentItem));
        }

        public event EventHandler<MediaItem> CurrentItemChanged;
        
        internal void MoveNext()
        {
            if (CanMoveNext)
            {
                currentItemIndex = NextItemIndex;
                CurrentItemChanged?.Invoke(this, (CurrentItem));
            }
        }
        internal void MoveTo(int index)
        {
            if(index >= 0 && index < Items.Count)
            {
                currentItemIndex = index;
                CurrentItemChanged?.Invoke(this, (CurrentItem));
            }
        }
        public bool CanMoveNext
        {
            get
            {
                return NextItemIndex > -1;
            }
        }

        internal void MovePrevious()
        {
            if (CanMovePrevious)
            {
                currentItemIndex = PreviousItemIndex;
                CurrentItemChanged?.Invoke(this, CurrentItem);
            }
        }
        public bool CanMovePrevious
        {
            get
            {
                return CurrentItemIndex > -1;
            }
        }

        public bool Shuffle { get; set; } = false;
        public string RepeatMode { get; set; } = "ALL";

        public List<MediaItem> Items { get; private set; } = new List<MediaItem>();

        public int NextItemIndex
        {
            get
            {
                if (CurrentItemIndex < 0)
                    return -1;
                if (CurrentItemIndex < Items.Count - 1)
                    return CurrentItemIndex + 1;
                else if (RepeatMode == "ALL")
                    return 0;
                else
                    return -1;
            }
        }
        public int PreviousItemIndex
        {
            get
            {
                if (CurrentItemIndex < 0)
                    return -1;
                if (CurrentItemIndex == 0 && (RepeatMode == "ALL"))
                    return Items.Count - 1;
                else
                    return -1;
            }
        }

        private int currentItemIndex;
        public int CurrentItemIndex
        {
            get
            {
                if (Items.Count == 0) return -1;
                return currentItemIndex;
            }
        }

        public MediaItem CurrentItem
        {
            get
            {
                if (CurrentItemIndex >= 0)
                    return Items[CurrentItemIndex];
                return null;
            }
        }
        public MediaItem NextItem
        {
            get
            {
                if (NextItemIndex >= 0)
                    return Items[NextItemIndex];
                return null;
            }
        }
    }
}
