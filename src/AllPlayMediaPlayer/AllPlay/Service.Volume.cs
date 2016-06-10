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
    public partial class Service : IVolumeService
    {
        VolumeProducer volume;

        IAsyncOperation<VolumeAdjustVolumeResult> IVolumeService.AdjustVolumeAsync(AllJoynMessageInfo info, short interfaceMemberDelta)
        {
            return Task.FromResult(VolumeAdjustVolumeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<VolumeAdjustVolumePercentResult> IVolumeService.AdjustVolumePercentAsync(AllJoynMessageInfo info, double interfaceMemberChange)
        {
            return Task.FromResult(VolumeAdjustVolumePercentResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<VolumeGetEnabledResult> IVolumeService.GetEnabledAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(VolumeGetEnabledResult.CreateSuccessResult(true)).AsAsyncOperation();
        }

        IAsyncOperation<VolumeGetMuteResult> IVolumeService.GetMuteAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(VolumeGetMuteResult.CreateSuccessResult(false)).AsAsyncOperation();
        }

        IAsyncOperation<VolumeSetMuteResult> IVolumeService.SetMuteAsync(AllJoynMessageInfo info, bool value)
        {
            return Task.FromResult(VolumeSetMuteResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<VolumeGetVersionResult> IVolumeService.GetVersionAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(VolumeGetVersionResult.CreateSuccessResult(1)).AsAsyncOperation();
        }

        IAsyncOperation<VolumeGetVolumeResult> IVolumeService.GetVolumeAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(VolumeGetVolumeResult.CreateSuccessResult(0)).AsAsyncOperation();
        }

        IAsyncOperation<VolumeSetVolumeResult> IVolumeService.SetVolumeAsync(AllJoynMessageInfo info, short value)
        {
            return Task.FromResult(VolumeSetVolumeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<VolumeGetVolumeRangeResult> IVolumeService.GetVolumeRangeAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(VolumeGetVolumeRangeResult.CreateSuccessResult(new VolumeVolumeRange() { Value1 = 0, Value2 = 100, Value3 = 1 })).AsAsyncOperation();
        }
    }
}
