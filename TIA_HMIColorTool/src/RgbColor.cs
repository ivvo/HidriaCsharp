using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace TIA_HMIColorTool
{
    /// <summary>RGB barva v TIA obliki "R, G, B".</summary>
    struct RgbColor : IEquatable<RgbColor>
    {
        public readonly int R, G, B;

        public RgbColor(int r, int g, int b)
        {
            if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
                throw new ArgumentOutOfRangeException("RGB vrednosti morajo biti 0-255.");
            R = r; G = g; B = b;
        }

        static readonly Regex ParseRx = new Regex(@"^\s*(\d{1,3})\s*[,;\s]\s*(\d{1,3})\s*[,;\s]\s*(\d{1,3})\s*$");

        /// <summary>Sprejme "29, 83, 131", "29,83,131", "29 83 131" ali "#1D5383".</summary>
        public static bool TryParse(string text, out RgbColor color)
        {
            color = default(RgbColor);
            if (string.IsNullOrWhiteSpace(text)) return false;
            text = text.Trim();

            if (text.StartsWith("#") && text.Length == 7)
            {
                try
                {
                    color = new RgbColor(Convert.ToInt32(text.Substring(1, 2), 16),
                                         Convert.ToInt32(text.Substring(3, 2), 16),
                                         Convert.ToInt32(text.Substring(5, 2), 16));
                    return true;
                }
                catch { return false; }
            }

            var m = ParseRx.Match(text);
            if (!m.Success) return false;
            int r = int.Parse(m.Groups[1].Value), g = int.Parse(m.Groups[2].Value), b = int.Parse(m.Groups[3].Value);
            if (r > 255 || g > 255 || b > 255) return false;
            color = new RgbColor(r, g, b);
            return true;
        }

        public Color ToColor() { return Color.FromArgb(R, G, B); }
        public static RgbColor FromColor(Color c) { return new RgbColor(c.R, c.G, c.B); }

        public override string ToString() { return R + ", " + G + ", " + B; }
        public bool Equals(RgbColor o) { return R == o.R && G == o.G && B == o.B; }
        public override bool Equals(object obj) { return obj is RgbColor && Equals((RgbColor)obj); }
        public override int GetHashCode() { return (R << 16) | (G << 8) | B; }
    }
}
