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
        private VolumeProducer volume;

        /// <summary>
        /// Adjust the volume by the given number. The adjustment can be up 
        /// (positive value) or down (negative value).
        /// </summary>
        /// <param name="info"></param>
        /// <param name="delta">Number of increments to adjust.</param>
        /// <returns></returns>
        IAsyncOperation<VolumeAdjustVolumeResult> IVolumeService.AdjustVolumeAsync(AllJoynMessageInfo info, short delta)
        {
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                player.Volume += delta / 100d;
            });
            return Task.FromResult(VolumeAdjustVolumeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<VolumeAdjustVolumePercentResult> IVolumeService.AdjustVolumePercentAsync(AllJoynMessageInfo info, double change)
        {
            // The change has floating point values between - 1.0 and 1.0 to represent volume
            // changes between - 100 % to 100 %.
            // A positive value (respectively negative), will increase (respectively decrease) the volume
            // by the percentage of the “remaining range” towards the maximum(respectively
            // minimum) value, i.e.difference between the current volume and the maximum
            // (respectively minimum) volume.
            // For example, when the volume range is [0 - 100] and we want to adjust by +50 %:
            // - If the current volume is 25, the increment will be:
            // “(100 - 25) * 50 %= 75 * 0.5 = 38” (once rounded) so the new volume will be 63.
            // - Another adjustment by + 50 % will be “(100 - 63) * 0.5 = 19” to a volume of 82.
            // If we want instead to adjust by -50 %, the decrement would be “(25 - 0) * 0.5 = 13” to a
            // volume of 12, and another adjustment by -50 % would be “(12 - 0) * 0.5 = 6” to a volume of 6.
            // This behavior provides a better user experience when changing the volume of multiple
            // speakers(group).At the same time, although each speaker has a different starting point,
            // all the players will reach 100 % (or 0 %) at the same time.
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                player.Volume += (1 - player.Volume) * change;
            });
            return Task.FromResult(VolumeAdjustVolumePercentResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<VolumeGetEnabledResult> IVolumeService.GetEnabledAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(VolumeGetEnabledResult.CreateSuccessResult(true)).AsAsyncOperation();
        }

        IAsyncOperation<VolumeGetMuteResult> IVolumeService.GetMuteAsync(AllJoynMessageInfo info)
        {
            var tcs = new TaskCompletionSource<VolumeGetMuteResult>();
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var isMuted = player.IsMuted;
                tcs.SetResult(VolumeGetMuteResult.CreateSuccessResult(isMuted));
            });
            return tcs.Task.AsAsyncOperation();
        }

        IAsyncOperation<VolumeSetMuteResult> IVolumeService.SetMuteAsync(AllJoynMessageInfo info, bool value)
        {
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                player.IsMuted = value;
            });
            return Task.FromResult(VolumeSetMuteResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<VolumeGetVolumeResult> IVolumeService.GetVolumeAsync(AllJoynMessageInfo info)
        {
            TaskCompletionSource<VolumeGetVolumeResult> tcs = new TaskCompletionSource<VolumeGetVolumeResult>();
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                short volume = (short)(player.Volume * 100d);
                tcs.SetResult(VolumeGetVolumeResult.CreateSuccessResult(volume));
            });
            return tcs.Task.AsAsyncOperation();
        }

        IAsyncOperation<VolumeSetVolumeResult> IVolumeService.SetVolumeAsync(AllJoynMessageInfo info, short value)
        {
            var _ = player.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                player.Volume = value / 100d;
            });
            return Task.FromResult(VolumeSetVolumeResult.CreateSuccessResult()).AsAsyncOperation();
        }

        IAsyncOperation<VolumeGetVolumeRangeResult> IVolumeService.GetVolumeRangeAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(VolumeGetVolumeRangeResult.CreateSuccessResult(new VolumeVolumeRange() { Value1 = 0, Value2 = 100, Value3 = 1 })).AsAsyncOperation();
        }

        IAsyncOperation<VolumeGetVersionResult> IVolumeService.GetVersionAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(VolumeGetVersionResult.CreateSuccessResult(1)).AsAsyncOperation();
        }
    }
}
