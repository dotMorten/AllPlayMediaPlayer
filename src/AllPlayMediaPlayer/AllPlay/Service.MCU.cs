using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using net.allplay.MCU;

namespace AllPlayMediaPlayer.AllPlay
{
    public partial class Service : IMCUService
    {
        private MCUProducer mcuProducer;

        IAsyncOperation<MCUAdvanceLoopModeResult> IMCUService.AdvanceLoopModeAsync(AllJoynMessageInfo info)
        {
            if (Playlist.RepeatMode == "NONE")
                Playlist.RepeatMode = "ALL";
            else if (Playlist.RepeatMode == "ALL")
                Playlist.RepeatMode = "SINGLE";
            else //if (Playlist.RepeatMode == "SINGLE")
                Playlist.RepeatMode = "NONE";
            return Task.FromResult(MCUAdvanceLoopModeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MCUGetCurrentItemUrlResult> IMCUService.GetCurrentItemUrlAsync(AllJoynMessageInfo info)
        {
            var current = Playlist.CurrentItem?.Url ?? "";
            return Task.FromResult(MCUGetCurrentItemUrlResult.CreateSuccessResult(current)).AsAsyncOperation();
        }

        IAsyncOperation<MCUPlayItemResult> IMCUService.PlayItemAsync(AllJoynMessageInfo info, string url, string title, string artist, string thumbnailUrl, long duration, string album, string genre)
        {
            // Question: Will this replace the current playlist, or append and skip to it?
            int index = Playlist.Enqueue(new net.allplay.MediaPlayer.MediaItem[]
            {
                new net.allplay.MediaPlayer.MediaItem()
                {
                    Album = album, Artist = artist, Duration = duration, Genre = genre,
                    MediaType = "upnp",
                    MediumDesc = new Dictionary<string,object>(),
                    OtherData = new Dictionary<string,string>(),
                    ThumbnailUrl = thumbnailUrl, Title = title, Url = url, UserData = ""
                }
            });
            Playlist.MoveTo(index);
            return Task.FromResult(MCUPlayItemResult.CreateFailureResult((int)QStatus.ER_FEATURE_NOT_AVAILABLE)).AsAsyncOperation();
        }

        IAsyncOperation<MCUSetExternalSourceResult> IMCUService.SetExternalSourceAsync(AllJoynMessageInfo info, string name, bool interruptible, bool volumeCtrlEnabled)
        {
            // TODO
            return Task.FromResult(MCUSetExternalSourceResult.CreateFailureResult((int)QStatus.ER_FEATURE_NOT_AVAILABLE)).AsAsyncOperation();
        }

        IAsyncOperation<MCUToggleShuffleModeResult> IMCUService.ToggleShuffleModeAsync(AllJoynMessageInfo info)
        {
            Playlist.Shuffle = !Playlist.Shuffle;
            return Task.FromResult(MCUToggleShuffleModeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<MCUGetVersionResult> IMCUService.GetVersionAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(MCUGetVersionResult.CreateSuccessResult(1)).AsAsyncOperation();
        }
    }
}
