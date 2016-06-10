using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using net.allplay.ZoneManager;

namespace AllPlayMediaPlayer.AllPlay
{
    public partial class Service : IZoneManagerService
    {
        ZoneManagerProducer zoneProducer;

        IAsyncOperation<ZoneManagerCreateZoneResult> IZoneManagerService.CreateZoneAsync(AllJoynMessageInfo info, IReadOnlyList<string> interfaceMemberSlaves)
        {
            //TODO
            return Task.FromResult(ZoneManagerCreateZoneResult.CreateFailureResult(1)).AsAsyncOperation();
        }

        IAsyncOperation<ZoneManagerGetEnabledResult> IZoneManagerService.GetEnabledAsync(AllJoynMessageInfo info)
        {
            //TODO: Make true when zone manager is supported
            return Task.FromResult(ZoneManagerGetEnabledResult.CreateSuccessResult(false)).AsAsyncOperation();
        }

        IAsyncOperation<ZoneManagerGetVersionResult> IZoneManagerService.GetVersionAsync(AllJoynMessageInfo info)
        {
            return Task.FromResult(ZoneManagerGetVersionResult.CreateSuccessResult(1)).AsAsyncOperation();
        }

        IAsyncOperation<ZoneManagerSetZoneLeadResult> IZoneManagerService.SetZoneLeadAsync(AllJoynMessageInfo info, string interfaceMemberZoneId, string interfaceMemberTimeServerIp, ushort interfaceMemberTimeServerPort)
        {
            //TODO
            return Task.FromResult(ZoneManagerSetZoneLeadResult.CreateFailureResult(1)).AsAsyncOperation();
        }
    }
}
