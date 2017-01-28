using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2WorldGenerator.Library
{
    public static class ColorUtility
    {
        public static Color CreateColorFromRange(int index, int maxIndex, Color minColor, Color maxColor)
        {
            int minR = minColor.R, minG = minColor.G, minB = minColor.B;
            int rangeR = maxColor.R - minR + 1;
            int rangeG = maxColor.G - minG + 1;
            int rangeB = maxColor.B - minB + 1;
            int colorsTotal = rangeR * rangeG * rangeB;
            int step = colorsTotal / (maxIndex + 1);
            int offset = step * index;
            int r = minR + offset % rangeR;
            int g = minG + offset / rangeR % rangeG;
            int b = minB + offset / rangeR / rangeG % rangeB;
            
            return Color.FromArgb(r, g, b);
        }
    }
}
