using LifxNet.Producs;
using System.Threading.Tasks;

namespace LifxNet
{
    public partial class LifxClient
    {
        public async Task<LightInfo> Resolve(LifxClient.DeviceDiscoveryEventArgs args)
        {
            var bulb = args.Device as LightBulb;
            var version = await GetDeviceVersionAsync(bulb);
            var state = await GetLightStateAsync(bulb);
            var firmware = await GetDeviceHostFirmwareAsync(bulb);

            LifxProduct? product = null;
            foreach (var v in vendors)
            {
                if (v.vid == version.Vendor)
                {
                    foreach (var p in v.products)
                    {
                        if (p.pid == version.Product)
                        {
                            product = p;
                            break;
                        }
                    }
                }
            }

            var light = new LightInfo(bulb, state, firmware, product);

            if (light.SupportsExtendedMultiZone)
            {
                var zones = await GetExtendedColorZonesAsync(bulb);
                light.Zones = zones.Colors;
            }
            else if (light.SupportsMultiZone)
            {
                var zones = await GetColorZonesAsync(bulb);
                light.Zones = zones.Colors;
            }
            return light;
        }
    }
}