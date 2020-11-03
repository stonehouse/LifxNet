using System;
using System.Threading.Tasks;

namespace LifxNet
{
    public partial class LifxClient
    {
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
    }
}
