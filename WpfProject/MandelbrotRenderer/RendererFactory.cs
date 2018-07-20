using ImageProcessor.ImageFilters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MandelbrotRenderer
{
    public static class RendererFactory
    {
        public static Rgba32[] GetRender(int iterations, int width, int height, double panX, double panY, double zoom)
        {
            //using (var file = File.Create(path)) { }

            var image = new Image<Rgba32>(width, height);

            var values = new Rgba32[width * height];
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using (var ilGpuFilter = new MandelbrotGpuFilter())
            {
                try
                {
                    values = ilGpuFilter.Apply(iterations, width, height, panX, panY, zoom);
                }
                catch (Exception exc)
                {
                    Debugger.Break();
                }
            }

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            return values;
        }
    }
}
