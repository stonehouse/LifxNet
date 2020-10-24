using LifxNet.Producs;
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
            var version = await GetDeviceVersionAsync(bulb);
            var state = await GetLightStateAsync(bulb);
            var firmware = await GetDeviceHostFirmwareAsync(bulb);

            LifxProduct? product = null;
            foreach (var v in vendors)
            {
                if (v.vid == version.Vendor)
                {
                    foreach(var p in v.products)
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
            return light;
        }

        public struct FirmwareVersion
        {
            public ushort major;
            public ushort minor;
        }

        public class LightInfo
        {
            public string Label { get; }
            public LightBulb Info { get; }
            public HSBK InitialColor { get; }
            public bool On { get; }
            public FirmwareVersion FirmwareVersion { get; }

            public string ID
            {
                get
                {
                    return Info.MacAddressName;
                }
            }
            public LifxProduct? Product;

            public LightInfo(LightBulb light, LightStateResponse state, StateHostFirmwareResponse firmware, LifxProduct? product)
            {
                this.Info = light;
                this.Label = state.Label;
                this.On = state.IsOn;
                this.InitialColor = state.Color;

                var firmwareBytes = BitConverter.GetBytes(firmware.Version);
                this.FirmwareVersion = new FirmwareVersion
                {
                    major = BitConverter.ToUInt16(firmwareBytes, 2),
                    minor = BitConverter.ToUInt16(firmwareBytes, 0)
                };
                this.Product = product;
            }

            public bool SupportsExtendedMultiZone
            {
                get
                {
                    if (Product != null)
                    {
                        var product = Product.Value;
                        if (product.features.multizone)
                        {
                            if (product.features.min_ext_mz_firmware_components != null && product.features.min_ext_mz_firmware_components.Length == 2)
                            {
                                return FirmwareVersion.major == product.features.min_ext_mz_firmware_components[0] 
                                    && FirmwareVersion.minor == product.features.min_ext_mz_firmware_components[1];
                            } 
                        }
                    }
                    return false;
                }
            }

            public bool SupportsMultiZone
            {
                get
                {
                    return Product?.features.multizone ?? false;
                }
            }
        }
    }
}