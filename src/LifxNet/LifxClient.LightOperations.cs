using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LifxNet
{
	public partial class LifxClient : IDisposable
	{
		private Dictionary<UInt32, Action<LifxResponse>> taskCompletions = new Dictionary<uint, Action<LifxResponse>>();

		/// <summary>
		/// Turns a bulb on using the provided transition time
		/// </summary>
		/// <param name="bulb"></param>
		/// <param name="transitionDuration"></param>
		/// <returns></returns>
		public Task TurnBulbOnAsync(LightBulb bulb, TimeSpan transitionDuration)
		{
			System.Diagnostics.Debug.WriteLine("Sending TurnBulbOn to {0}", bulb.HostName);
			return SetLightPowerAsync(bulb, transitionDuration, true);
		}
		/// <summary>
		/// Turns a bulb off using the provided transition time
		/// </summary>
		public Task TurnBulbOffAsync(LightBulb bulb, TimeSpan transitionDuration)
		{
			System.Diagnostics.Debug.WriteLine("Sending TurnBulbOff to {0}", bulb.HostName);
			return SetLightPowerAsync(bulb, transitionDuration, false);
		}
		private async Task SetLightPowerAsync(LightBulb bulb, TimeSpan transitionDuration, bool isOn)
		{
			if (bulb == null)
				throw new ArgumentNullException("bulb");
			if (transitionDuration.TotalMilliseconds > UInt32.MaxValue ||
				transitionDuration.Ticks < 0)
				throw new ArgumentOutOfRangeException("transitionDuration");

			FrameHeader header = new FrameHeader()
			{
				Identifier = (uint)randomizer.Next(),
				AcknowledgeRequired = true
			};

			var b = BitConverter.GetBytes((UInt16)transitionDuration.TotalMilliseconds);

			await BroadcastMessageAsync<AcknowledgementResponse>(bulb.HostName, header, MessageType.LightSetPower,
				(UInt16)(isOn ? 65535 : 0), b
			).ConfigureAwait(false);
		}
		/// <summary>
		/// Gets the current power state for a light bulb
		/// </summary>
		/// <param name="bulb"></param>
		/// <returns></returns>
		public async Task<bool> GetLightPowerAsync(LightBulb bulb)
		{
			FrameHeader header = new FrameHeader()
			{
				Identifier = (uint)randomizer.Next(),
				AcknowledgeRequired = true
			};
			return (await BroadcastMessageAsync<LightPowerResponse>(
				bulb.HostName, header, MessageType.LightGetPower).ConfigureAwait(false)).IsOn;
		}

		/// <summary>
		/// Sets color and temperature for a bulb
		/// </summary>
		/// <param name="bulb"></param>
		/// <param name="color"></param>
		/// <param name="kelvin"></param>
		/// <returns></returns>
		public Task SetColorAsync(LightBulb bulb, Color color, UInt16 kelvin)
		{
			return SetColorAsync(bulb, color, kelvin, TimeSpan.Zero);
		}
		/// <summary>
		/// Sets color and temperature for a bulb and uses a transition time to the provided state
		/// </summary>
		/// <param name="bulb"></param>
		/// <param name="color"></param>
		/// <param name="kelvin"></param>
		/// <param name="transitionDuration"></param>
		/// <returns></returns>
		public Task SetColorAsync(LightBulb bulb, Color color, UInt16 kelvin, TimeSpan transitionDuration)
		{
			var hsl = Utilities.RgbToHsl(color);
			return SetColorAsync(bulb, hsl[0], hsl[1], hsl[2], kelvin, transitionDuration);
		}

		/// <summary>
		/// Sets color and temperature for a bulb and uses a transition time to the provided state
		/// </summary>
		/// <param name="bulb">Light bulb</param>
		/// <param name="hue">0..65535</param>
		/// <param name="saturation">0..65535</param>
		/// <param name="brightness">0..65535</param>
		/// <param name="kelvin">2700..9000</param>
		/// <param name="transitionDuration"></param>
		/// <returns></returns>
		public async Task SetColorAsync(LightBulb bulb,
			UInt16 hue,
			UInt16 saturation,
			UInt16 brightness,
			UInt16 kelvin,
			TimeSpan transitionDuration)
		{
			if (transitionDuration.TotalMilliseconds > UInt32.MaxValue ||
				transitionDuration.Ticks < 0)
				throw new ArgumentOutOfRangeException("transitionDuration");
			if (kelvin < 2500 || kelvin > 9000)
			{
				throw new ArgumentOutOfRangeException("kelvin", "Kelvin must be between 2500 and 9000");
			}

				System.Diagnostics.Debug.WriteLine("Setting color to {0}", bulb.HostName);
			FrameHeader header = new FrameHeader()
			{
				Identifier = (uint)randomizer.Next(),
				AcknowledgeRequired = true
			};
			UInt32 duration = (UInt32)transitionDuration.TotalMilliseconds;
			var durationBytes = BitConverter.GetBytes(duration);
			var h = BitConverter.GetBytes(hue);
			var s = BitConverter.GetBytes(saturation);
			var b = BitConverter.GetBytes(brightness);
			var k = BitConverter.GetBytes(kelvin);

			await BroadcastMessageAsync<AcknowledgementResponse>(bulb.HostName, header,
				MessageType.LightSetColor, (byte)0x00, //reserved
					hue, saturation, brightness, kelvin, //HSBK
					duration
			);
		}

		/*
		public async Task SetBrightnessAsync(LightBulb bulb,
			UInt16 brightness,
			TimeSpan transitionDuration)
		{
			if (transitionDuration.TotalMilliseconds > UInt32.MaxValue ||
				transitionDuration.Ticks < 0)
				throw new ArgumentOutOfRangeException("transitionDuration");

			FrameHeader header = new FrameHeader()
			{
				Identifier = (uint)randomizer.Next(),
				AcknowledgeRequired = true
			};
			UInt32 duration = (UInt32)transitionDuration.TotalMilliseconds;
			var durationBytes = BitConverter.GetBytes(duration);
			var b = BitConverter.GetBytes(brightness);

			await BroadcastMessageAsync<AcknowledgementResponse>(bulb.HostName, header,
				MessageType.SetLightBrightness, brightness, duration
			);
		}*/

			/// <summary>
			/// Gets the current state of the bulb
			/// </summary>
			/// <param name="bulb"></param>
			/// <returns></returns>
		public Task<LightStateResponse> GetLightStateAsync(LightBulb bulb)
		{
			FrameHeader header = new FrameHeader()
			{
				Identifier = (uint)randomizer.Next(),
				AcknowledgeRequired = false
			};
			return BroadcastMessageAsync<LightStateResponse>(
				bulb.HostName, header, MessageType.LightGet);
		}

        public async Task SetColorZonesAsync(LightBulb bulb, byte index, Color color, UInt16 kelvin, UInt32 duration, ZoneApplicationRequest apply)
        {
            await SetColorZonesAsync(bulb, index, index, color, kelvin, duration, apply);
        }

        public async Task SetColorZonesAsync(LightBulb bulb, byte startIndex, byte endIndex, Color color, UInt16 kelvin, UInt32 duration, ZoneApplicationRequest apply)
        {
            var hsl = Utilities.RgbToHsl(color);
            await SetColorZonesAsync(bulb, startIndex, endIndex, new HSBK(hsl[0], hsl[1], hsl[2], kelvin), duration, apply);
        }

        public async Task SetColorZonesAsync(LightBulb bulb, byte startIndex, byte endIndex, HSBK color, UInt32 duration, ZoneApplicationRequest apply)
        {
            FrameHeader header = new FrameHeader()
            {
                Identifier = (uint)randomizer.Next(),
                AcknowledgeRequired = true
            };

            var h = BitConverter.GetBytes(color.Hue);
            var s = BitConverter.GetBytes(color.Saturation);
            var b = BitConverter.GetBytes(color.Brightness);
            var k = BitConverter.GetBytes(color.Kelvin);
            var d = BitConverter.GetBytes(duration);

            await BroadcastMessageAsync<AcknowledgementResponse>(
                bulb.HostName, header, MessageType.LightSetColorZones, startIndex, endIndex, h, s, b, k, d, (byte)apply);
        }

        public Task<LightStateMultiZoneResponse> GetColorZonesAsync(LightBulb bulb, byte startIndex, byte endIndex)
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
