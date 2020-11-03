using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace LifxNet
{
    public partial class LifxClient
    {
        public async Task SetColorZonesAsync(LightBulb bulb, byte index, Color color, UInt16 kelvin, UInt32 duration, ZoneApplicationRequest apply)
        {
            await SetColorZonesAsync(bulb, index, index, color, kelvin, duration, apply);
        }

        public async Task SetColorZonesAsync(LightBulb bulb, byte startIndex, byte endIndex, Color color, UInt16 kelvin, UInt32 duration, ZoneApplicationRequest apply)
        {
            var hsl = Utilities.RgbToHsl(color);
            await SetColorZonesAsync<AcknowledgementResponse>(bulb, startIndex, endIndex, new HSBK(hsl[0], hsl[1], hsl[2], kelvin), duration, apply);
        }

        public void SetColorZones(LightBulb bulb, byte index, Color color, UInt16 kelvin, UInt32 duration, ZoneApplicationRequest apply)
        {
            SetColorZones(bulb, index, index, color, kelvin, duration, apply);
        }

        public void SetColorZones(LightBulb bulb, byte startIndex, byte endIndex, Color color, UInt16 kelvin, UInt32 duration, ZoneApplicationRequest apply)
        {
            var hsl = Utilities.RgbToHsl(color);
            SetColorZonesAsync<UnknownResponse>(bulb, startIndex, endIndex, new HSBK(hsl[0], hsl[1], hsl[2], kelvin), duration, apply).ContinueWith((fin) => { });
        }

        private async Task<T> SetColorZonesAsync<T>(LightBulb bulb, byte startIndex, byte endIndex, HSBK color, UInt32 duration, ZoneApplicationRequest apply) where T : LifxResponse
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = typeof(T) == typeof(AcknowledgementResponse)
            };

            var h = BitConverter.GetBytes(color.Hue);
            var s = BitConverter.GetBytes(color.Saturation);
            var b = BitConverter.GetBytes(color.Brightness);
            var k = BitConverter.GetBytes(color.Kelvin);
            var d = BitConverter.GetBytes(duration);

            return await BroadcastMessageAsync<T>(
                bulb.HostName, header, MessageType.LightSetColorZones, startIndex, endIndex, h, s, b, k, d, (byte)apply);
        }

        private Task<LightStateMultiZoneResponse> GetColorZonesAsync(LightBulb bulb, byte startIndex, byte endIndex)
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = false
            };

            return BroadcastMessageAsync<LightStateMultiZoneResponse>(
                bulb.HostName, header, MessageType.LightGetColorZones, startIndex, endIndex);
        }

        public async Task<LightStateMultiZoneResponse> GetColorZonesAsync(LightBulb bulb)
        {
            var colors = new List<HSBK>();
            byte increment = 8;
            byte zonesLoaded = 0;
            byte zonesCount = 255;
            while (zonesLoaded < zonesCount)
            {
                var endIndex = (byte)(zonesLoaded + increment);
                var zonesResponse = await GetColorZonesAsync(bulb, zonesLoaded, endIndex);
                colors.AddRange(zonesResponse.Colors);
                zonesCount = zonesResponse.Count;
                zonesLoaded += endIndex;
            }

            return new LightStateMultiZoneResponse(zonesCount, colors.ToArray());
        }
    }
}