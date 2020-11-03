using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifxNet
{
    public partial class LifxClient
    {
        public void SetExtendedColorZonesAsync(LightBulb bulb, Color[] colors, UInt16 kelvin, UInt32 duration)
        {
            var hsbks = new List<HSBK>();
            foreach (var color in colors)
            {
                var hsl = Utilities.RgbToHsl(color);
                var hsbk = new HSBK(hsl[0], hsl[1], hsl[2], kelvin);
                hsbks.Add(hsbk);
            }

            SetExtendedColorZonesAsync(bulb, hsbks.ToArray(), duration);
        }

        public void SetExtendedColorZonesAsync(LightBulb bulb, HSBK[] colors, UInt32 duration)
        {
            SetExtendedColorZonesAsync<UnknownResponse>(bulb, colors, duration).ContinueWith((fin) => { });
        }

        private async Task<T> SetExtendedColorZonesAsync<T>(LightBulb bulb, HSBK[] colors, UInt32 duration) where T : LifxResponse
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = typeof(T) == typeof(AcknowledgementResponse)
            };

            var args = new List<byte>();

            var d = BitConverter.GetBytes(duration);
            args.AddRange(d);

            var apply = ZoneApplicationRequest.Apply;
            args.Add((byte)apply);

            UInt16 index = 0;
            args.AddRange(BitConverter.GetBytes(index));

            args.Add((byte)colors.Length);

            foreach (var color in colors)
            {
                var h = BitConverter.GetBytes(color.Hue);
                var s = BitConverter.GetBytes(color.Saturation);
                var b = BitConverter.GetBytes(color.Brightness);
                var k = BitConverter.GetBytes(color.Kelvin);

                args.AddRange(h);
                args.AddRange(s);
                args.AddRange(b);
                args.AddRange(k);
            }

            return await BroadcastMessageAsync<T>(
                bulb.HostName, header, MessageType.MultiZoneExtendedSetZones, args.ToArray());
        }

        public Task<LightStateExtendedMultiZoneResponse> GetExtendedColorZonesAsync(LightBulb bulb)
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = false
            };

            return BroadcastMessageAsync<LightStateExtendedMultiZoneResponse>(
                bulb.HostName, header, MessageType.MultiZoneExtendedGetZones, new object[0]);
        }
    }
}
