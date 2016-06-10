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
        MCUProducer mcuProducer;

        IAsyncOperation<MCUAdvanceLoopModeResult> IMCUService.AdvanceLoopModeAsync(AllJoynMessageInfo info)
        {
            //TODO
            return Task.FromResult(MCUAdvanceLoopModeResult.CreateFailureResult(1)).AsAsyncOperation();
        }

        IAsyncOperation<MCUGetCurrentItemUrlResult> IMCUService.GetCurrentItemUrlAsync(AllJoynMessageInfo info)
        {
            //TODO
            return Task.FromResult(MCUGetCurrentItemUrlResult.CreateFailureResult(1)).AsAsyncOperation();
        }

        IAsyncOperation<MCUGetVersionResult> IMCUService.GetVersionAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(MCUGetVersionResult.CreateSuccessResult(1)).AsAsyncOperation();
        }

        IAsyncOperation<MCUPlayItemResult> IMCUService.PlayItemAsync(AllJoynMessageInfo info, string interfaceMemberUrl, string interfaceMemberTitle, string interfaceMemberArtist, string interfaceMemberThumbnailUrl, long interfaceMemberDuration, string interfaceMemberAlbum, string interfaceMemberGenre)
        {
            //TODO
            return Task.FromResult(MCUPlayItemResult.CreateFailureResult(1)).AsAsyncOperation();
        }

        IAsyncOperation<MCUSetExternalSourceResult> IMCUService.SetExternalSourceAsync(AllJoynMessageInfo info, string interfaceMemberName, bool interfaceMemberInterruptible, bool interfaceMemberVolumeCtrlEnabled)
        {
            //TODO
            return Task.FromResult(MCUSetExternalSourceResult.CreateFailureResult(1)).AsAsyncOperation();
        }

        IAsyncOperation<MCUToggleShuffleModeResult> IMCUService.ToggleShuffleModeAsync(AllJoynMessageInfo info)
        {
            //TODO
            return Task.FromResult(MCUToggleShuffleModeResult.CreateFailureResult(1)).AsAsyncOperation();
        }
    }
}
