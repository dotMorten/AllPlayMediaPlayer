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
        net.allplay.MediaPlayer.MediaPlayerProducer producer;
        IAsyncOperation<MediaPlayerForcedPreviousResult> IMediaPlayerService.ForcedPreviousAsync(AllJoynMessageInfo info)
        {
            playlist.MovePrevious();

            return Task.FromResult(MediaPlayerForcedPreviousResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetPlayerInfoResult> IMediaPlayerService.GetPlayerInfoAsync(AllJoynMessageInfo info)
        {
            Dictionary<string, int> slaveNames = new Dictionary<string, int>();
            slaveNames.Add("Slave1", 1234);
            slaveNames.Add("Slave2", 1235);
            return Task.FromResult(MediaPlayerGetPlayerInfoResult.CreateSuccessResult("MediaPlayer", new List<string> { "audio", "video", "supportsPartyMode" }, 100,
                new MediaPlayerZoneInfo() { Value1 = "l5lvqDYz7HYX3b0f", Value2 = 0, Value3 = slaveNames}
                )).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetPlaylistInfoResult> IMediaPlayerService.GetPlaylistInfoAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(MediaPlayerGetPlaylistInfoResult.CreateSuccessResult("com.qualcomm.qce.allplay.jukebox-7d4bbc2d197e4447", "")).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerNextResult> IMediaPlayerService.NextAsync(AllJoynMessageInfo info)
        {
            playlist.MoveNext();
            return Task.FromResult(MediaPlayerNextResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerPauseResult> IMediaPlayerService.PauseAsync(AllJoynMessageInfo info)
        {
            if(player.CanPause)
                player.Pause();
            return Task.FromResult(MediaPlayerPauseResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerPlayResult> IMediaPlayerService.PlayAsync(AllJoynMessageInfo info, int itemIndex, long startPositionMsecs, bool pauseStateOnly)
        {
            //TODO: Set item in playlist
            playlist.MoveTo((uint)itemIndex);
            if (player.CanSeek)
                player.Position = TimeSpan.FromMilliseconds(startPositionMsecs);
            player.Play();
            return Task.FromResult(MediaPlayerPlayResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerPreviousResult> IMediaPlayerService.PreviousAsync(AllJoynMessageInfo info)
        {
            if (player.Position.TotalSeconds > 2 && player.CanSeek)
                player.Position = TimeSpan.Zero;
            else
            {
                playlist.MovePrevious();
            }
            return Task.FromResult(MediaPlayerPreviousResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerResumeResult> IMediaPlayerService.ResumeAsync(AllJoynMessageInfo info)
        {
            player.Play();
            return Task.FromResult(MediaPlayerResumeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerSetPositionResult> IMediaPlayerService.SetPositionAsync(AllJoynMessageInfo info, long interfaceMemberPositionMsecs)
        {
            if (player.CanSeek)
                player.Position = TimeSpan.FromMilliseconds(interfaceMemberPositionMsecs);
            return Task.FromResult(MediaPlayerSetPositionResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerStopResult> IMediaPlayerService.StopAsync(AllJoynMessageInfo info)
        {
            player.Pause();
            //if(player.CanSeek)
            //    player.Position = TimeSpan.Zero;
            return Task.FromResult(MediaPlayerStopResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetEnabledControlsResult> IMediaPlayerService.GetEnabledControlsAsync(AllJoynMessageInfo info)
        {
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
            return Task.FromResult(MediaPlayerGetInterruptibleResult.CreateSuccessResult(true)).AsAsyncOperation();
            //return Task.FromResult(MediaPlayerGetInterruptibleResult.CreateSuccessResult(player.CanPause)).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetLoopModeResult> IMediaPlayerService.GetLoopModeAsync(AllJoynMessageInfo info)
        {
            string mode = "";
            switch (player.SystemMediaTransportControls.AutoRepeatMode)
            {
                case Windows.Media.MediaPlaybackAutoRepeatMode.None:
                    mode = "NONE"; break;
                case Windows.Media.MediaPlaybackAutoRepeatMode.Track:
                    mode = "SINGLE"; break;
                case Windows.Media.MediaPlaybackAutoRepeatMode.List:
                default:
                    mode = "ALL"; break;
            }
            return Task.FromResult(MediaPlayerGetLoopModeResult.CreateSuccessResult(mode)).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerSetLoopModeResult> IMediaPlayerService.SetLoopModeAsync(AllJoynMessageInfo info, string value)
        {
            value = value.ToUpper();
            switch(value)
            {
                case "NONE":
                    player.SystemMediaTransportControls.AutoRepeatMode = Windows.Media.MediaPlaybackAutoRepeatMode.None; break;
                case "SINGLE":
                    player.SystemMediaTransportControls.AutoRepeatMode = Windows.Media.MediaPlaybackAutoRepeatMode.Track; break;
                case "ALL":
                default:
                    player.SystemMediaTransportControls.AutoRepeatMode = Windows.Media.MediaPlaybackAutoRepeatMode.List; break;
            }
            return Task.FromResult(MediaPlayerSetLoopModeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetPlayStateResult> IMediaPlayerService.GetPlayStateAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(MediaPlayerGetPlayStateResult.CreateSuccessResult(GetCurrentState())).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetShuffleModeResult> IMediaPlayerService.GetShuffleModeAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(MediaPlayerGetShuffleModeResult.CreateSuccessResult(player.SystemMediaTransportControls.ShuffleEnabled ? "SHUFFLE" : "LINEAR")).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerSetShuffleModeResult> IMediaPlayerService.SetShuffleModeAsync(AllJoynMessageInfo info, string value)
        {
            player.SystemMediaTransportControls.ShuffleEnabled = value.Equals("SHUFFLE", StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(MediaPlayerSetShuffleModeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetVersionResult> IMediaPlayerService.GetVersionAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(MediaPlayerGetVersionResult.CreateSuccessResult(1)).AsAsyncOperation();
        }

        IAsyncOperation<MediaPlayerGetPlaylistResult> IMediaPlayerService.GetPlaylistAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(MediaPlayerGetPlaylistResult.CreateSuccessResult(mediaItems, "MyController", "")).AsAsyncOperation();
        }
    }
}
