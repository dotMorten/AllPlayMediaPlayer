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
    public partial class Service : net.allplay.MediaPlayer.IMediaPlayerService
    {
        net.allplay.MediaPlayer.MediaPlayerProducer mediaProducer;
        IAsyncOperation<MediaPlayerForcedPreviousResult> IMediaPlayerService.ForcedPreviousAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.ForcedPreviousAsync() @ {info?.SenderUniqueName ?? "null"}");
            Playlist.MovePrevious();
            return Task.FromResult(MediaPlayerForcedPreviousResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetPlayerInfoResult> IMediaPlayerService.GetPlayerInfoAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.GetPlayerInfoAsync() @ {info?.SenderUniqueName ?? "null"}");
            Dictionary<string, int> slaveNames = new Dictionary<string, int>();
            slaveNames.Add("Slave1", 1234);
            slaveNames.Add("Slave2", 1235);
            return Task.FromResult(MediaPlayerGetPlayerInfoResult.CreateSuccessResult(
                "MediaPlayer",
                // TODO: If running headless, only support audio
                new List<string> { "audio/mpeg", "video/mp4", "video/mpeg", "image/jpeg", "supportsPartyMode" },
                100,
                new MediaPlayerZoneInfo() { Value1 = GetUniqueDeviceId(), Value2 = 0, Value3 = slaveNames }
                )).AsAsyncOperation();
        }

        private static string GetUniqueDeviceId()
        {
            var container = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("MediaPlayer", Windows.Storage.ApplicationDataCreateDisposition.Always);
            if(container.Values.ContainsKey("ZoneID"))
            {
                return (string)container.Values["ZoneID"];
            }
            // We didn't have one stored. Generate a new one
            var id = Guid.NewGuid().ToString().Replace("-", "");
            container.Values["ZoneID"] = id;
            return id;
        }

        IAsyncOperation<MediaPlayerGetPlaylistInfoResult> IMediaPlayerService.GetPlaylistInfoAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.GetPlaylistInfoAsync() @ {info?.SenderUniqueName ?? "null"}");
            return Task.FromResult(MediaPlayerGetPlaylistInfoResult.CreateSuccessResult("com.qualcomm.qce.allplay.jukebox-7d4bbc2d197e4447", "")).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerNextResult> IMediaPlayerService.NextAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.NextAsync() @ {info?.SenderUniqueName ?? "null"}");
            Playlist.MoveNext();
            return Task.FromResult(MediaPlayerNextResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerPauseResult> IMediaPlayerService.PauseAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.PauseAsync() @ {info?.SenderUniqueName ?? "null"}");
            //if(player.CanPause)
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                player.Pause();
            });
            return Task.FromResult(MediaPlayerPauseResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerPlayResult> IMediaPlayerService.PlayAsync(AllJoynMessageInfo info, int itemIndex, long startPositionMsecs, bool pauseStateOnly)
        {
            if(itemIndex < 0 || itemIndex>=Playlist.Items.Count)
                return Task.FromResult(MediaPlayerPlayResult.CreateFailureResult((int)QStatus.ER_BAD_ARG_1)).AsAsyncOperation();
            if (startPositionMsecs < 0)
                return Task.FromResult(MediaPlayerPlayResult.CreateFailureResult((int)QStatus.ER_BAD_ARG_2)).AsAsyncOperation();
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.PlayAsync() @ {info?.SenderUniqueName ?? "null"}");
            //TODO: Set item in playlist
            Playlist.MoveTo(itemIndex);
            //if (player.CanSeek)
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                player.Position = TimeSpan.FromMilliseconds(startPositionMsecs);
                if (!pauseStateOnly)
                    player.Play();
            });
            return Task.FromResult(MediaPlayerPlayResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerPreviousResult> IMediaPlayerService.PreviousAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.PreviousAsync() @ {info?.SenderUniqueName ?? "null"}");
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (player.Position.TotalSeconds > 2 && player.CanSeek)
                    player.Position = TimeSpan.Zero;
                else
                {
                    Playlist.MovePrevious();
                }
            });
            return Task.FromResult(MediaPlayerPreviousResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerResumeResult> IMediaPlayerService.ResumeAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.ResumeAsync() @ {info?.SenderUniqueName ?? "null"}");
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                player.Play();
            });
            return Task.FromResult(MediaPlayerResumeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerSetPositionResult> IMediaPlayerService.SetPositionAsync(AllJoynMessageInfo info, long interfaceMemberPositionMsecs)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.SetPositionAsync({interfaceMemberPositionMsecs}) @ {info?.SenderUniqueName ?? "null"}");
            //if (player.CanSeek)
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                player.Position = TimeSpan.FromMilliseconds(interfaceMemberPositionMsecs);
            });
            return Task.FromResult(MediaPlayerSetPositionResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerStopResult> IMediaPlayerService.StopAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.StopAsync() @ {info?.SenderUniqueName ?? "null"}");
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                player.Stop();
            });
            //if(player.CanSeek)
            //    player.Position = TimeSpan.Zero;
            return Task.FromResult(MediaPlayerStopResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetEnabledControlsResult> IMediaPlayerService.GetEnabledControlsAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.GetEnabledControlsAsync() @ {info?.SenderUniqueName ?? "null"}");
            return Task.FromResult(MediaPlayerGetEnabledControlsResult.CreateSuccessResult(
                new Dictionary<string, bool>() {
                    { "loopMode",true },
                    { "next",true },
                    { "previous",true },
                    { "seek", true },
                    { "shuffleMode",true },
                })).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetInterruptibleResult> IMediaPlayerService.GetInterruptibleAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.GetInterruptibleAsync() @ {info?.SenderUniqueName ?? "null"}");
            return Task.FromResult(MediaPlayerGetInterruptibleResult.CreateSuccessResult(true)).AsAsyncOperation();
            //return Task.FromResult(MediaPlayerGetInterruptibleResult.CreateSuccessResult(player.CanPause)).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetLoopModeResult> IMediaPlayerService.GetLoopModeAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.GetLoopModeAsync() @ {info?.SenderUniqueName ?? "null"}");
            string mode = Playlist.RepeatMode;
            return Task.FromResult(MediaPlayerGetLoopModeResult.CreateSuccessResult(mode)).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerSetLoopModeResult> IMediaPlayerService.SetLoopModeAsync(AllJoynMessageInfo info, string value)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.SetLoopModeAsync({value}) @ {info?.SenderUniqueName ?? "null"}");
            value = value.ToUpper();
            if(value != "NONE" || value!= "SINGLE" || value != "ALL")
                Playlist.RepeatMode = value;
            RaiseStateChanged();
            return Task.FromResult(MediaPlayerSetLoopModeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetPlayStateResult> IMediaPlayerService.GetPlayStateAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.GetPlayStateAsync() @ {info?.SenderUniqueName ?? "null"}");
            return Task.FromResult(MediaPlayerGetPlayStateResult.CreateSuccessResult(GetCurrentState())).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetShuffleModeResult> IMediaPlayerService.GetShuffleModeAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.GetShuffleModeAsync() @ {info?.SenderUniqueName ?? "null"}");
            return Task.FromResult(MediaPlayerGetShuffleModeResult.CreateSuccessResult(Playlist.Shuffle ? "SHUFFLE" : "LINEAR")).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerSetShuffleModeResult> IMediaPlayerService.SetShuffleModeAsync(AllJoynMessageInfo info, string value)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.SetShuffleModeAsync({value}) @ {info?.SenderUniqueName ?? "null"}");
            Playlist.Shuffle = value.Equals("SHUFFLE", StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(MediaPlayerSetShuffleModeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetVersionResult> IMediaPlayerService.GetVersionAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.GetVersionAsync() @ {info?.SenderUniqueName ?? "null"}");
            return Task.FromResult(MediaPlayerGetVersionResult.CreateSuccessResult(1)).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetPlaylistResult> IMediaPlayerService.GetPlaylistAsync(AllJoynMessageInfo info)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.GetPlaylistAsync() @ {info?.SenderUniqueName ?? "null"}");
            return Task.FromResult(MediaPlayerGetPlaylistResult.CreateSuccessResult(Playlist.Items, "MyController", "")).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerUpdatePlaylistResult> IMediaPlayerService.UpdatePlaylistAsync(AllJoynMessageInfo info, IList<MediaItem> playlistItems, int index, string controllerType, string playlistUserData)
        {
            System.Diagnostics.Debug.WriteLine($"IMediaPlayerService.UpdatePlaylistAsync(items[{playlistItems.Count},{index},\"{controllerType}\",\"{playlistUserData}\") @ {info?.SenderUniqueName ?? "null"}");
            foreach(var item in playlistItems)
            {
                item.UserData = string.Empty;
            }
            //Note: These needs to be cloned
            var mediaItems = new List<MediaItem>(playlistItems.Select(m =>
                new MediaItem()
                {
                    Album = m.Album,
                    Duration = m.Duration,
                    Genre = m.Genre,
                    Artist = m.Artist,
                    MediaType = m.MediaType,
                    MediumDesc = m.MediumDesc,
                    OtherData = m.OtherData,
                    ThumbnailUrl = m.ThumbnailUrl,
                    Title = m.Title,
                    Url = m.Url,
                    UserData = " "
                }
                ));
            Playlist.UpdatePlaylist(mediaItems);
            mediaProducer.Signals.PlaylistChanged();
            return Task.FromResult(MediaPlayerUpdatePlaylistResult.CreateSuccessResult()).AsAsyncOperation();
        }
    }
}
