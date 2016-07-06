using net.allplay.MediaPlayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllPlayMediaPlayer.AllPlay
{
    public class Playlist : INotifyPropertyChanged
    {
        Windows.UI.Core.CoreDispatcher dispatcher;
        public Playlist()
        {
            dispatcher = App.Current?.Resources?.Dispatcher;
        }
        public int Enqueue(IEnumerable<MediaItem> playlist)
        {
            int result = Items.Count;
            UpdatePlaylist(playlist, Items.Count);
            return result;
        }
        public void UpdatePlaylist(IEnumerable<MediaItem> playlist, int index)
        {
            if (index == 0)
            {
                Items = new List<MediaItem>(playlist);
            }
            else
            {
                if (index < Items.Count)
                    Items.RemoveRange(index, Items.Count - index);
                Items.InsertRange(index, playlist);
                if (currentItemIndex > Items.Count)
                    currentItemIndex = 0;
            }
            RaiseCurrentItemChanged();
            Save();
        }

        public void LoadFromCache()
        {
            if (!Windows.Storage.ApplicationData.Current.LocalSettings.Containers.ContainsKey("Playlist"))
                return;
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("Playlist", Windows.Storage.ApplicationDataCreateDisposition.Existing);
            int count = (int)settings.Values["Count"];
            for (int i = 0; i < count; i++)
            {
                var item = (string)settings.Values[i.ToString()];
                Items.Add(DeserializeMediaItem(item));
            }
        }
        public void Save()
        {
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("Playlist", Windows.Storage.ApplicationDataCreateDisposition.Always);
            settings.Values.Clear();
            int i = 0;
            settings.Values["Count"] = Items.Count;
            foreach(var item in Items)
            {
                settings.Values[i.ToString()] = SerializeMediaItem(item);
                i++;
            }
        }

        private MediaItem DeserializeMediaItem(string s)
        {
            MediaItem mi = new MediaItem();
            var vals = s.Split(new char[] { '\n' });
            mi.Album = vals[0].Trim();
            mi.Artist = vals[1].Trim();
            mi.Duration = long.Parse(vals[2].Trim());
            mi.Genre = vals[3].Trim();
            mi.MediaType = vals[4].Trim();
            mi.MediumDesc = new Dictionary<string, object>(); // vals[5].Trim();
            mi.OtherData = new Dictionary<string, string>(); // vals[6].Trim();
            mi.ThumbnailUrl = vals[7].Trim();
            mi.Title = vals[8].Trim();
            mi.Url = vals[9].Trim();
            mi.UserData = vals[10].Trim();
            if (mi.UserData as string == "") mi.UserData = "upnp";
            return mi;
        }

        private string SerializeMediaItem(MediaItem item)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(item.Album);
            sb.AppendLine(item.Artist);
            sb.AppendLine(item.Duration.ToString());
            sb.AppendLine(item.Genre);
            sb.AppendLine(item.MediaType);
            sb.AppendLine(); //item.MediumDesc);
            sb.AppendLine(); //item.OtherData);
            sb.AppendLine(item.ThumbnailUrl);
            sb.AppendLine(item.Title);
            sb.AppendLine(item.Url);
            sb.AppendLine(item.UserData as string);
            return sb.ToString();
        }

        private void RaiseCurrentItemChanged()
        {
            CurrentItemChanged?.Invoke(this, CurrentItem);
            if (dispatcher == null || dispatcher.HasThreadAccess)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentItem)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextItem)));
            }
            else
            {
                var _ = dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentItem)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextItem)));
                });
            }
        }

        public event EventHandler<MediaItem> CurrentItemChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        internal void MoveNext()
        {
            if (CanMoveNext)
            {
                currentItemIndex = NextItemIndex;
                RaiseCurrentItemChanged();
            }
        }

        internal void MoveTo(int index)
        {
            if(index >= 0 && index < Items.Count)
            {
                currentItemIndex = index;
                RaiseCurrentItemChanged();
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
                RaiseCurrentItemChanged();
            }
        }
        public bool CanMovePrevious
        {
            get
            {
                return CurrentItemIndex > -1;
            }
        }

        private bool m_shuffle;

        public bool Shuffle
        {
            get { return m_shuffle; }
            set
            {
                if (m_shuffle != value)
                {
                    m_shuffle = value;
                    ShuffleModeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ShuffleModeChanged;

        private string _repeatMode = "ALL";

        public string RepeatMode
        {
            get { return _repeatMode; }
            set
            {
                if (_repeatMode != value)
                {
                    _repeatMode = value;
                    RepeatModeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler RepeatModeChanged;

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
