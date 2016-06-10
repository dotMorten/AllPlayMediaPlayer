using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using net.allplay.MediaPlayer;
using net.allplay.Control.Volume;
using Windows.Devices.AllJoyn;
using Windows.Foundation;

namespace AllPlayMediaPlayer.AllPlay
{
    // https://blogs.windows.com/buildingapps/2016/01/13/the-basics-of-background-audio/

    public partial class Service 
    {
        private Windows.Media.Playback.MediaPlayer player;
        private AllJoynBusAttachment bus;
        private Windows.Media.Playback.MediaPlaybackList playlist;

        public Service()
        {
            player = Windows.Media.Playback.BackgroundMediaPlayer.Current;
            player.AutoPlay = false;
            playlist = new Windows.Media.Playback.MediaPlaybackList()
            {
                AutoRepeatEnabled = true
            };
            LoadPlaylist();
            player.Source = playlist;
            bus = new AllJoynBusAttachment();

            //IntPtr ptr2 = System.Runtime.InteropServices.Marshal.GetIUnknownForObject(bus.AboutData);
            //AllJoynInterop.alljoyn_aboutobj_unannounce(ptr2);
            //AllJoynInterop.alljoyn_aboutobjectdescription_haspath(ptr2, "sdad");
            //AllJoynInterop.alljoyn_aboutdata_setdeviceid(ptr2, "MyPlayerID");

            bus.AboutData.IsEnabled = true;
            bus.AboutData.DefaultAppName = "AllPlay Player"; // Windows.ApplicationModel.Package.Current.DisplayName;
            bus.AboutData.DefaultManufacturer = Windows.ApplicationModel.Package.Current.PublisherDisplayName;
            bus.AboutData.DefaultDescription = "AllPlay Player Runtime"; // Windows.ApplicationModel.Package.Current.Description;
            bus.AboutData.ModelNumber = "1";
            var version = Windows.ApplicationModel.Package.Current.Id.Version;
            bus.AboutData.SoftwareVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            bus.AboutData.SupportUrl = new Uri("http://github.com/dotMorten");

            allplayProducer = new net.allplay.AllPlayProducer(bus);
            //producer = new net.allplay.MediaPlayer.MediaPlayerProducer(bus);
            producer = allplayProducer.MediaPlayer;
            producer.Service = this;

            volume = allplayProducer.Volume; // new VolumeProducer(bus);
            volume.Service = this;

            mcuProducer = allplayProducer.MCU;// new net.allplay.MCU.MCUProducer(bus);
            mcuProducer.Service = this;

            zoneProducer = allplayProducer.ZoneManager;// new net.allplay.ZoneManager.ZoneManagerProducer(bus);
            zoneProducer.Service = this;

        }
        List<MediaItem> mediaItems = new List<MediaItem>();
        private void LoadPlaylist()
        {
            for (int i = 0; i < 20; i++)
            {
                var item = CreateMockMediaItem(i);
                mediaItems.Add(item);
                playlist.Items.Add(new Windows.Media.Playback.MediaPlaybackItem(Windows.Media.Core.MediaSource.CreateFromUri(new Uri(item.Url))));
            }
        }
        private static MediaItem CreateMockMediaItem(int id)
        {
            return new MediaItem()
            {
                Url = $"http://google.com/{id}/mp3",
                Album = "Album",
                Artist = "Artist",
                Duration = 10000,
                Genre = "Genre",
                MediaType = "mp3",
                MediumDesc = new Dictionary<string, object>(),
                ThumbnailUrl = "http://google.com/thumbnail.jpg",
                Title = "Title " + id.ToString(),
                OtherData = new Dictionary<string, string>(),
                UserData = "upnp"
            };
        }

        net.allplay.AllPlayProducer allplayProducer;
        public void Start()
        {
            //bus.Connect();
            allplayProducer.Start();

            //player.BufferingEnded += Player_BufferingEnded;
            //player.BufferingStarted += Player_BufferingStarted;
            //player.CurrentStateChanged += Player_CurrentStateChanged;
            //player.MediaEnded += Player_MediaEnded;
            //player.MediaFailed += Player_MediaFailed;
            //player.MediaOpened += Player_MediaOpened;
            //player.VolumeChanged += Player_VolumeChanged;
            //player.SystemMediaTransportControls.AutoRepeatModeChangeRequested += SystemMediaTransportControls_AutoRepeatModeChangeRequested;
            //player.SystemMediaTransportControls.ShuffleEnabledChangeRequested += SystemMediaTransportControls_ShuffleEnabledChangeRequested;
            playlist.CurrentItemChanged += Playlist_CurrentItemChanged;
            //Loop();
        }

        //private async void Loop()
        //{
        //    DateTime time = DateTime.Now;
        //    while (true)
        //    {
        //        RaiseStateChanged();
        //    }
        //}
        
        private void Playlist_CurrentItemChanged(Windows.Media.Playback.MediaPlaybackList sender, Windows.Media.Playback.CurrentMediaPlaybackItemChangedEventArgs args)
        {
            RaiseStateChanged();
        }
        private void RaiseStateChanged()
        {
            producer.Signals.PlayStateChanged(GetCurrentState());
        }
        DateTime startTime = DateTime.Now;
        private MediaPlayerPlayState GetCurrentState()
        {
            var items = new List<MediaItem>();
            int next = 0;
            items.Add(mediaItems[0]);
            items.Add(mediaItems[1]);
            if (playlist.CurrentItem != null)
            {
                items.Add(mediaItems[(int)playlist.CurrentItemIndex]);
                if (!playlist.ShuffleEnabled)
                {
                    next = (int)playlist.CurrentItemIndex + 1;
                    if (next >= items.Count && playlist.AutoRepeatEnabled)
                        next = 0;
                    else next = -1;
                    if (next >= 0)
                    {
                        items.Add(mediaItems[next]);
                    }
                }
            }
            return new MediaPlayerPlayState()
            {
                CurrentIndex =  0, // (int)playlist.CurrentItemIndex,
                MediaItems = items,
                AudioChannels = 2,
                NextIndex = next,
                Position = 0, //(long)(DateTime.Now - startTime).TotalMilliseconds,
                SampleRate = 0,
                BitsPerSample = 0,
                State = PlayerStateToString(player.CurrentState),
            };
        }
        private static string PlayerStateToString(Windows.Media.Playback.MediaPlayerState state)
        {
            switch(state)
            {
                case Windows.Media.Playback.MediaPlayerState.Buffering: return "BUFFERING";
                case Windows.Media.Playback.MediaPlayerState.Opening: return "TRANSITIONING";
                case Windows.Media.Playback.MediaPlayerState.Paused: return "PAUSED";
                case Windows.Media.Playback.MediaPlayerState.Playing: return "PLAYING";
                case Windows.Media.Playback.MediaPlayerState.Stopped:
                case Windows.Media.Playback.MediaPlayerState.Closed:
                default:
                    return "STOPPED";
            }
        }

        private void SystemMediaTransportControls_ShuffleEnabledChangeRequested(Windows.Media.SystemMediaTransportControls sender, Windows.Media.ShuffleEnabledChangeRequestedEventArgs args)
        {
            producer.Signals.ShuffleModeChanged(args.RequestedShuffleEnabled ? "SHUFFLE" : "LINEAR");
        }

        private void SystemMediaTransportControls_AutoRepeatModeChangeRequested(Windows.Media.SystemMediaTransportControls sender, Windows.Media.AutoRepeatModeChangeRequestedEventArgs args)
        {
            string mode = "";
            switch (args.RequestedAutoRepeatMode)
            {
                case Windows.Media.MediaPlaybackAutoRepeatMode.None:
                    mode = "NONE"; break;
                case Windows.Media.MediaPlaybackAutoRepeatMode.Track:
                    mode = "SINGLE"; break;
                case Windows.Media.MediaPlaybackAutoRepeatMode.List:
                default:
                    mode = "ALL"; break;
            }
            producer.Signals.LoopModeChanged(mode);
        }

        private void Player_VolumeChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            volume.Signals.VolumeChanged((short)player.Volume);
        }

        private void Player_MediaOpened(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            RaiseStateChanged();
        }

        private void Player_MediaFailed(Windows.Media.Playback.MediaPlayer sender, Windows.Media.Playback.MediaPlayerFailedEventArgs args)
        {
            producer.Signals.OnPlaybackError((int)playlist.CurrentItemIndex, args.Error.GetType().Name, args.ErrorMessage);
        }

        private void Player_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            RaiseStateChanged();
            producer.Signals.EndOfPlayback();
            //producer.Signals.OnPlayStateChanged(...)
        }

        private void Player_CurrentStateChanged(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            RaiseStateChanged();
        }

        private void Player_BufferingStarted(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            RaiseStateChanged();
        }

        private void Player_BufferingEnded(Windows.Media.Playback.MediaPlayer sender, object args)
        {
            RaiseStateChanged();
        }

        public string Manifest { get; private set; }
    }
}
