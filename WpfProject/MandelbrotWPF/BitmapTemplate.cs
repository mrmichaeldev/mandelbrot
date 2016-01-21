using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotWPF.Bitmaptemplate
{
    public struct Color
    {
        public byte R, G, B;
        public bool ShouldNotCalculate;
    }
    public struct BitmapTemplate
    {
        public int Width;
        public int Height;
        public Color[,] Image;
    }
}
