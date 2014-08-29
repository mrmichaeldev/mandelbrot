using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotWPF
{
    public static class Generator
    {
        const double xmin = -2.1;
        const double ymin = -1.3;
        const double xmax = 1;
        const double ymax = 1.3;

        const double intigralX = (xmax - xmin) / Width / Zoom;
        const double intigralY = (ymax - ymin) / Height / Zoom;

        const double scrollX = (-2.2d + panX * Zoom) * (xmax - xmin) / Zoom / 2 * Height / Width;//-2d + (xmax - xmin) / Width;
        const double scrollY = (-1.5d + panY * Zoom) * (xmax - xmin) / Zoom / 2 * Height / Width;//-1.3d + (xmax - xmin) / Height / Height;

        const int iterations = 500;

        public static Bitmap Generate() { }
    }
}
