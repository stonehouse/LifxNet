using System;
using System.Linq;

namespace LifxNet
{
    public partial class LifxClient
    {
        public void SetColors(LightInfo light, Color[] colors, UInt16 kelvin)
        {
            if (light.SupportsExtendedMultiZone)
            {
                var spread = SpreadColors(colors, light.Zones.Length);
                SetExtendedColorZonesAsync(light.Info, colors, kelvin, 0);
            }
            else if (light.SupportsMultiZone)
            {
                var count = light.Zones.Length;
                SetZones(light.Info, colors, kelvin, count);
            }
            else
            {
                SetColor(light.Info, colors.First(), kelvin);
            }
        }

        private Color[] SpreadColors(Color[] colors, int zonesCount)
        {
            var zonesPerColor = zonesCount / colors.Length;
            var zoneColors = new Color[zonesCount];
            var zonesAssignedToColor = 0;

            var colorList = colors.ToList();
            Color? color = colorList[0];
            colorList.RemoveAt(0);
            for (var i = 0; i < zonesCount; i++)
            {
                if (zonesAssignedToColor > zonesPerColor && colorList.Count() > 0)
                {
                    zonesAssignedToColor = 0;
                    color = colorList[0];
                    colorList.RemoveAt(0);
                }

                zonesAssignedToColor++;

                zoneColors[i] = color.Value;
            }

            return zoneColors;
        }

        private void SetZones(LightBulb light, Color[] colors, int kelvin, int zonesCount)
        {
            var zonesPerColor = zonesCount / colors.Length;
            var zoneColors = new Color[zonesCount];
            var startIndex = 0;
            var endIndex = zonesPerColor - 1;

            var colorList = colors.ToList();
            Color? color = colorList[0];
            colorList.RemoveAt(0);
            for (var i = 0; i < colors.Length; i++)
            {
                SetColorZones(light, Convert.ToByte(startIndex), Convert.ToByte(endIndex), colors[i], Convert.ToUInt16(kelvin), 0, ZoneApplicationRequest.Apply);

                startIndex = endIndex + 1;
                endIndex += zonesPerColor;
                if (endIndex >= zonesCount)
                {
                    endIndex = zonesCount - 1;
                }
                else if (endIndex + zonesPerColor > (zonesCount - 1))
                {
                    endIndex = zonesCount - 1;
                }
            }
        }
    }
}
