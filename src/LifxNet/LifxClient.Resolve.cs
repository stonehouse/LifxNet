using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifxNet
{
	public partial class LifxClient
	{
        public async Task<LightInfo> Resolve(LifxClient.DeviceDiscoveryEventArgs args)
        {
            var bulb = args.Device as LightBulb;
            var state = await GetLightStateAsync(bulb);
            var firmware = await GetDeviceHostFirmwareAsync(bulb);

            var light = new LightInfo(bulb, state, firmware);
            return light;
        }

        public class LightInfo
        {
            public string Label { get; }
            public LightBulb Info { get; }
            public HSBK InitialColor { get; }
            public bool On { get; }
            public UInt32 FirmwareVersion { get; }
            public string ID
            {
                get
                {
                    return Info.MacAddressName;
                }
            }

            public LightInfo(LightBulb light, LightStateResponse state, StateHostFirmwareResponse firmware)
            {
                this.Info = light;
                this.Label = state.Label;
                this.On = state.IsOn;
                this.InitialColor = state.Color;
                this.FirmwareVersion = firmware.Version;

            }
        }
    }
}