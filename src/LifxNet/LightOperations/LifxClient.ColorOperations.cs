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
            await SetColorAsync<AcknowledgementResponse>(bulb, hue, saturation, brightness, kelvin, transitionDuration);
        }

        public void SetColor(LightBulb bulb, Color color, UInt16 kelvin)
        {
            SetColor(bulb, color, kelvin, TimeSpan.Zero);
        }

        public void SetColor(LightBulb bulb, Color color, UInt16 kelvin, TimeSpan transitionDuration)
        {
            var hsl = Utilities.RgbToHsl(color);
            SetColorAsync<UnknownResponse>(bulb, hsl[0], hsl[1], hsl[2], kelvin, transitionDuration).ContinueWith((fin) => { });
        }
        
        public void SetColor(LightBulb bulb,
            UInt16 hue,
            UInt16 saturation,
            UInt16 brightness,
            UInt16 kelvin,
            TimeSpan transitionDuration)
        {
            SetColorAsync<UnknownResponse>(bulb, hue, saturation, brightness, kelvin, transitionDuration).ContinueWith((fin) => { });
        }

        private async Task<T> SetColorAsync<T>(LightBulb bulb,
			UInt16 hue,
			UInt16 saturation,
			UInt16 brightness,
			UInt16 kelvin,
			TimeSpan transitionDuration) where T: LifxResponse
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

		    return await BroadcastMessageAsync<T>(bulb.HostName, header,
				MessageType.LightSetColor, (byte)0x00, //reserved
					hue, saturation, brightness, kelvin, //HSBK
					duration
			);
		}

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
    }
}
