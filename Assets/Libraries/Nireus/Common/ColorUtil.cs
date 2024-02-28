using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus
{
    public static class ColorUtil
    {
        public static Color ColorWithHex(string hex)
        {
            if (!hex.StartsWith("#"))
            {
                hex = "#" + hex;
            }

            if (ColorUtility.TryParseHtmlString(hex, out var color))
            {
                return color;
            }
            else
            {
                return Color.black;
            }
        }
        
        ///Convert Hex to Color.
        private static Dictionary<string, Color> hexColorCache = new Dictionary<string, Color>(System.StringComparer.OrdinalIgnoreCase);
        public static Color HexToColor(string hex) {
            Color result;
            if ( hexColorCache.TryGetValue(hex, out result) ) {
                return result;
            }
            if ( hex.Length != 6 ) {
                throw new System.Exception("Invalid length for hex color provided");
            }
            var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            result = new Color32(r, g, b, 255);
            return hexColorCache[hex] = result;
        }
    }

}
