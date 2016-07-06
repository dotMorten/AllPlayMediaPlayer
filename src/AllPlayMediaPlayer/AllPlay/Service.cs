using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using net.allplay.MediaPlayer;
using net.allplay.Control.Volume;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace AllPlayMediaPlayer.AllPlay
{
    // https://blogs.windows.com/buildingapps/2016/01/13/the-basics-of-background-audio/

    public partial class Service 
    {
        //private Windows.Media.Playback.MediaPlayer player;
        private readonly AllJoynBusAttachment bus;
        private readonly net.allplay.AllPlayProducer allplayProducer;
        //private Windows.Media.Playback.MediaPlaybackList playlist;
        private MediaElement player;
        private DateTime startTime = DateTime.Now;
        //private MediaTransportControls transportControls;
        public Playlist Playlist { get; } = new Playlist();

        public Service(MediaElement mediaElement)
        {
            //player.Source = playlist;

            // Initialize the alljoyn bus
            bus = new AllJoynBusAttachment();
            bus.AboutData.IsEnabled = true;
            bus.AboutData.DefaultAppName = Windows.ApplicationModel.Package.Current.DisplayName;
            bus.AboutData.DefaultManufacturer = Windows.ApplicationModel.Package.Current.PublisherDisplayName;
            bus.AboutData.DefaultDescription = "AllPlay Player Runtime"; // Windows.ApplicationModel.Package.Current.Description;
            bus.AboutData.ModelNumber = "1";
            var version = Windows.ApplicationModel.Package.Current.Id.Version;
            bus.AboutData.SoftwareVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            bus.AboutData.SupportUrl = new Uri("https://github.com/dotMorten/AllPlayMediaPlayer/Issues");

            // Initialize up the producers
            allplayProducer = new net.allplay.AllPlayProducer(bus);
            mediaProducer = allplayProducer.MediaPlayer;
            mediaProducer.Service = this;
            volume = allplayProducer.Volume;
            volume.Service = this;
            mcuProducer = allplayProducer.MCU;
            mcuProducer.Service = this;
            zoneProducer = allplayProducer.ZoneManager;
            zoneProducer.Service = this;

#if DEBUG
            allplayProducer.Stopped += AllplayProducer_Stopped;
            allplayProducer.SessionLost += AllplayProducer_SessionLost;
            allplayProducer.SessionMemberAdded += AllplayProducer_SessionMemberAdded;
            allplayProducer.SessionMemberRemoved += AllplayProducer_SessionMemberRemoved;
#endif

            player = mediaElement;
            Playlist.CurrentItemChanged += Playlist_CurrentItemChanged;
            Playlist.ShuffleModeChanged += Playlist_ShuffleModeChanged;
            Playlist.RepeatModeChanged += Playlist_RepeatModeChanged;

            Playlist.LoadFromCache();
#if DEBUG
            if (Playlist.Items.Count == 0)
                LoadSamplePlaylist();
#endif
        }

#if DEBUG
        private void LoadSamplePlaylist()
        {
            var mediaItems = new List<MediaItem>();
            for (int i = 0; i < 20; i++)
            {
                var item = CreateMockMediaItem(i);
                mediaItems.Add(item);
                //playlist.Items.Add(new Windows.Media.Playback.MediaPlaybackItem(Windows.Media.Core.MediaSource.CreateFromUri(new Uri(item.Url))));
            }
            Playlist.UpdatePlaylist(mediaItems, 0);
        }

        private static MediaItem CreateMockMediaItem(int id)
        {
            return new MediaItem()
            {
                Url = id % 2 == 0 ? "http://www.tonycuffe.com/mp3/tail%20toddle.mp3" : "http://www.tonycuffe.com/mp3/cairnomount.mp3",
                Album = "Album",
                Artist = "Artist",
                Duration = (long)(id % 2 == 0 ? TimeSpan.FromSeconds(60 + 36) : TimeSpan.FromSeconds(60 + 26)).TotalMilliseconds,
                Genre = "Genre",
                MediaType = "mp3",
                MediumDesc = new Dictionary<string, object>(),
                ThumbnailUrl = "http://www.djparallelnyc.com/No-album-art-itunes.jpg",
                Title = "Title " + id.ToString(),
                OtherData = new Dictionary<string, string>(),
                UserData = "upnp"
            };
        }
#endif

        private void Playlist_CurrentItemChanged(object sender, MediaItem e)
        {
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                player.Source = Playlist.CurrentItem == null ? null : new Uri(Playlist.CurrentItem.Url);
                RaiseStateChanged();
            });
        }

        private void Playlist_ShuffleModeChanged(object sender, EventArgs e)
        {
            mediaProducer.Signals.ShuffleModeChanged(Playlist.Shuffle ? "SHUFFLE" : "LINEAR");
            RaiseStateChanged();
        }

        private void Playlist_RepeatModeChanged(object sender, EventArgs e)
        {
            mediaProducer.Signals.LoopModeChanged(Playlist.RepeatMode);
            RaiseStateChanged();
        }

#if DEBUG
        private void AllplayProducer_Stopped(net.allplay.AllPlayProducer sender, AllJoynProducerStoppedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.Producer stopped: {args.Status}");
        }

        private void AllplayProducer_SessionLost(net.allplay.AllPlayProducer sender, AllJoynSessionLostEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.Session Lost: {args.Reason}");
        }

        private void AllplayProducer_SessionMemberAdded(net.allplay.AllPlayProducer sender, AllJoynSessionMemberAddedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.Session Member Added: {args.UniqueName}");
        }

        private void AllplayProducer_SessionMemberRemoved(net.allplay.AllPlayProducer sender, AllJoynSessionMemberRemovedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.Session Member Removed: {args.UniqueName}");
        }
#endif

        public void Start()
        {
            allplayProducer.Start();

            player.CurrentStateChanged += Player_CurrentStateChanged;
            player.MediaFailed += Player_MediaFailed;
            player.MediaOpened += Player_MediaOpened;
            player.SeekCompleted += Player_SeekCompleted;
            player.MediaEnded += Player_MediaEnded;
            //player.BufferingProgressChanged += Player_BufferingProgressChanged;
            //player.BufferingEnded += Player_BufferingEnded;
            //player.BufferingStarted += Player_BufferingStarted;
            //player.VolumeChanged += Player_VolumeChanged;
            //player.SystemMediaTransportControls.AutoRepeatModeChangeRequested += SystemMediaTransportControls_AutoRepeatModeChangeRequested;
            //player.SystemMediaTransportControls.ShuffleEnabledChangeRequested += SystemMediaTransportControls_ShuffleEnabledChangeRequested;
            player.VolumeChanged += Player_VolumeChanged;
        }

        private void Player_MediaEnded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (Playlist.CanMoveNext)
                Playlist.MoveNext();
        }

        private void Player_MediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RaiseStateChanged();
        }

        private void Player_MediaFailed(object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs e)
        {
            RaiseStateChanged();
            mediaProducer.Signals.OnPlaybackError(Playlist.CurrentItemIndex, e.ErrorMessage, "");
        }

        // Store last known state of player so we can read them from background threads
        // without needing to switch to the UI thread
        private TimeSpan lastKnownPlayerPosition;
        private DateTime lastKnowPlayerPositionTimestamp;
        private Windows.UI.Xaml.Media.MediaElementState currentState = Windows.UI.Xaml.Media.MediaElementState.Closed;

        private void Player_CurrentStateChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            currentState = player.CurrentState;
            lastKnownPlayerPosition = player.Position;
            lastKnowPlayerPositionTimestamp = DateTime.Now;
            RaiseStateChanged();
        }

        private void Player_SeekCompleted(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // TODO: Throttle, since this fires very frequently if dragging the slider
            lastKnownPlayerPosition = player.Position;
            lastKnowPlayerPositionTimestamp = DateTime.Now;
            RaiseStateChanged();
        }

        private void Player_VolumeChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            volume.Signals.VolumeChanged((short)(player.Volume * 100));
        }
        
        private void RaiseStateChanged()
        {
            mediaProducer.Signals.PlayStateChanged(GetCurrentState());
        }

        private MediaPlayerPlayState GetCurrentState()
        {
            var items = new List<MediaItem>();
            if(Playlist.CurrentItem != null)
                items.Add(Playlist.CurrentItem);
            if (Playlist.NextItem != null)
                items.Add(Playlist.NextItem);

            long currentPosition = 0;
            if(currentState == Windows.UI.Xaml.Media.MediaElementState.Playing || 
                currentState == Windows.UI.Xaml.Media.MediaElementState.Paused ||
                currentState == Windows.UI.Xaml.Media.MediaElementState.Buffering)
                currentPosition = (long)(DateTime.Now - lastKnowPlayerPositionTimestamp + lastKnownPlayerPosition).TotalMilliseconds;

            return new MediaPlayerPlayState()
            {
                CurrentIndex = Math.Max(0, Playlist.CurrentItemIndex),
                MediaItems = items,
                AudioChannels = 2,
                NextIndex = Math.Max(0, Playlist.NextItemIndex),
                Position = currentPosition,
                SampleRate = 0,
                BitsPerSample = 0,
                State = PlayerStateToString(currentState),
            };
        }
        
        private static string PlayerStateToString(Windows.UI.Xaml.Media.MediaElementState state)
        {
            switch (state)
            {
                case Windows.UI.Xaml.Media.MediaElementState.Buffering: return "BUFFERING";
                case Windows.UI.Xaml.Media.MediaElementState.Opening: return "TRANSITIONING";
                case Windows.UI.Xaml.Media.MediaElementState.Paused: return "PAUSED";
                case Windows.UI.Xaml.Media.MediaElementState.Playing: return "PLAYING";
                case Windows.UI.Xaml.Media.MediaElementState.Stopped:
                case Windows.UI.Xaml.Media.MediaElementState.Closed:
                default:
                    return "STOPPED";
            }
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

        public string Manifest { get; private set; }
    }
}
