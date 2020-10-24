using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifxNet
{
	/// <summary>
	/// Base class for LIFX response types
	/// </summary>
	public abstract class LifxResponse
	{
		internal static LifxResponse Create(FrameHeader header, MessageType type, UInt32 source, byte[] payload)
		{
			LifxResponse response = null;
			switch(type)
			{
				case MessageType.DeviceAcknowledgement:
					response = new AcknowledgementResponse(payload);
					break;
				case MessageType.DeviceStateLabel:
					response = new StateLabelResponse(payload);
					break;
				case MessageType.LightState:
					response = new LightStateResponse(payload);
					break;
				case MessageType.LightStatePower:
					response = new LightPowerResponse(payload);
					break;
				case MessageType.DeviceStateVersion:
					response = new StateVersionResponse(payload);
					break;
				case MessageType.DeviceStateHostFirmware:
					response = new StateHostFirmwareResponse(payload);
					break;
				case MessageType.DeviceStateService:
					response = new StateServiceResponse(payload);
					break;
                case MessageType.LightStateMultiZone:
                    response = new LightStateMultiZoneResponse(payload);
                    break;
                case MessageType.LightStateZone:
                    response = new LightStateZoneResponse(payload);
                    break;
				case MessageType.MultiZoneExtendedStateZones:
					response = new LightStateExtendedMultiZoneResponse(payload);
					break;
                default:
					response = new UnknownResponse(payload);
					break;
			}
			response.Header = header;
			response.Type = type;
			response.Payload = payload;
			response.Source = source;
			return response;
		}
		internal LifxResponse() { }
		internal FrameHeader Header { get; private set; }
		internal byte[] Payload { get; private set; }
		internal MessageType Type { get; private set; }
		internal UInt32 Source { get; private set; }
	}
	/// <summary>
	/// Response to any message sent with ack_required set to 1. 
	/// </summary>
	internal class AcknowledgementResponse: LifxResponse
	{
		internal AcknowledgementResponse(byte[] payload) : base() { }
	}
	/// <summary>
	/// Response to GetService message.
	/// Provides the device Service and port.
	/// If the Service is temporarily unavailable, then the port value will be 0.
	/// </summary>
	internal class StateServiceResponse : LifxResponse
	{
		internal StateServiceResponse(byte[] payload) : base()
		{
			Service = payload[0];
			Port = BitConverter.ToUInt32(payload, 1);
		}
		public Byte Service { get; set; }
		public UInt32 Port { get; private set; }
	}
	/// <summary>
	/// Response to GetLabel message. Provides device label.
	/// </summary>
	internal class StateLabelResponse : LifxResponse
	{
		internal StateLabelResponse(byte[] payload) : base() {

			if (payload != null)
				Label = Encoding.UTF8.GetString(payload, 0, payload.Length).Replace("\0", "");
		}
		public string Label { get; private set; }
	}
	/// <summary>
	/// Sent by a device to provide the current light state
	/// </summary>
	public class LightStateResponse : LifxResponse
	{
		internal LightStateResponse(byte[] payload) : base()
		{
            var hsbkSize = 8;
            var bytes = new byte[hsbkSize];
            Array.Copy(payload, 0, bytes, 0, hsbkSize);
            Color = new HSBK(bytes);
            IsOn = BitConverter.ToUInt16(payload, 10) > 0;
			Label = Encoding.UTF8.GetString(payload, 12, 32).Replace("\0","");
		}
		/// <summary>
		/// Color
		/// </summary>
		public HSBK Color { get; private set; }
		/// <summary>
		/// Power state
		/// </summary>
		public bool IsOn { get; private set; }
		/// <summary>
		/// Light label
		/// </summary>
		public string Label { get; private set; }
	}
	internal class LightPowerResponse : LifxResponse
	{
		internal LightPowerResponse(byte[] payload) : base()
		{
			IsOn = BitConverter.ToUInt16(payload, 0) > 0;
		}
		public bool IsOn { get; private set; }
	}

	/// <summary>
	/// Response to GetVersion message.	Provides the hardware version of the device.
	/// </summary>
	public class StateVersionResponse : LifxResponse
	{
		internal StateVersionResponse(byte[] payload) : base()
		{
			Vendor = BitConverter.ToUInt32(payload, 0);
			Product = BitConverter.ToUInt32(payload, 4);
			Version = BitConverter.ToUInt32(payload, 8);
		}
		/// <summary>
		/// Vendor ID
		/// </summary>
		public UInt32 Vendor { get; private set; }
		/// <summary>
		/// Product ID
		/// </summary>
		public UInt32 Product { get; private set; }
		/// <summary>
		/// Hardware version
		/// </summary>
		public UInt32 Version { get; private set; }
	}
	/// <summary>
	/// Response to GetHostFirmware message. Provides host firmware information.
	/// </summary>
	public class StateHostFirmwareResponse : LifxResponse
	{
		internal StateHostFirmwareResponse(byte[] payload) : base()
		{
			var nanoseconds = BitConverter.ToUInt64(payload, 0);
			Build = Utilities.Epoch.AddMilliseconds(nanoseconds * 0.000001);
			//8..15 UInt64 is reserved
			Version = BitConverter.ToUInt32(payload, 16);
		}
		/// <summary>
		/// Firmware build time
		/// </summary>
		public DateTime Build { get; private set; }
		/// <summary>
		/// Firmware version
		/// </summary>
		public UInt32 Version { get; private set; }
	}

	internal class UnknownResponse : LifxResponse
	{
		internal UnknownResponse(byte[] payload) : base() {
		}
	}

    public struct HSBK
    {
		internal static int size = 8;
        public UInt16 Hue;
        /// <summary>
        /// Saturation (0=desaturated, 65535 = fully saturated)
        /// </summary>
        public UInt16 Saturation;
        /// <summary>
        /// Brightness (0=off, 65535=full brightness)
        /// </summary>
        public UInt16 Brightness;
        /// <summary>
        /// Bulb color temperature
        /// </summary>
        public UInt16 Kelvin;

        internal HSBK(byte[] payload)
        {
            Hue = BitConverter.ToUInt16(payload, 0);
            Saturation = BitConverter.ToUInt16(payload, 2);
            Brightness = BitConverter.ToUInt16(payload, 4);
            Kelvin = BitConverter.ToUInt16(payload, 6);
        }

        public HSBK(UInt16 hue, UInt16 saturation, UInt16 brightness, UInt16 kelvin)
        {
            Hue = hue;
            Saturation = saturation;
            Brightness = brightness;
            Kelvin = kelvin;
        }
    }

    public enum ZoneApplicationRequest: byte
    {
        NoApply = 0, Apply, ApplyOnly
    }

    public class LightStateMultiZoneResponse : LifxResponse
    {
        internal LightStateMultiZoneResponse(byte[] payload) : base()
        {
            var colorsCount = 8;
            Count = payload[0];
            Index = payload[1];
            Colors = new HSBK[colorsCount];
            var hsbkSize = 8;
            for (int i = 0; i < colorsCount; i++)
            {
                var bytes = new byte[hsbkSize];
                Array.Copy(payload, i * hsbkSize + 2, bytes, 0, hsbkSize);
                Colors[i] = new HSBK(bytes);
            }

        }

        public LightStateMultiZoneResponse(Byte count, HSBK[] colors)
        {
            Count = count;
            Index = 0;
            Colors = colors;
        }

        /// <summary>
        /// Zone Count
        /// </summary>
        public Byte Count { get; private set; }
        public Byte Index { get; private set; }

        public HSBK[] Colors { get; private set; }
    }

    public class LightStateZoneResponse : LifxResponse
    {
        internal LightStateZoneResponse(byte[] payload) : base()
        {
            Count = payload[0];
            Index = payload[1];
            var hsbkSize = 8;
            var bytes = new byte[hsbkSize];
            Array.Copy(payload, 2, bytes, 0, hsbkSize);
            Color = new HSBK(bytes);
        }
        /// <summary>
        /// Zone Count
        /// </summary>
        public Byte Count { get; private set; }
        public Byte Index { get; private set; }

        public HSBK Color { get; private set; }
    }

	public class LightStateExtendedMultiZoneResponse : LifxResponse
	{
		internal LightStateExtendedMultiZoneResponse(byte[] payload) : base()
		{
			Count = BitConverter.ToUInt16(payload, 0);
			Index = BitConverter.ToUInt16(payload, 2);
			var colorsCount = BitConverter.ToUInt16(payload, 4);
			Colors = new HSBK[colorsCount];
			for (int i = 0; i < colorsCount; i++)
			{
				var bytes = new byte[HSBK.size];
				Array.Copy(payload, i * HSBK.size + 2, bytes, 0, HSBK.size);
				Colors[i] = new HSBK(bytes);
			}

		}

		public LightStateExtendedMultiZoneResponse(Byte count, HSBK[] colors)
		{
			Count = count;
			Index = 0;
			Colors = colors;
		}

		/// <summary>
		/// Zone Count
		/// </summary>
		public UInt16 Count { get; private set; }
		public UInt16 Index { get; private set; }
		public HSBK[] Colors { get; private set; }
	}
}
