using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mandelbrot
{
    internal class Program
    {
        //private const double Zoom = 100d;
        private const double Zoom = 12000d;

        private static void Main(string[] args)
        {
            var file = File.Create(@"Mandelbrot.png");

            DrawMandel().Save(file, ImageFormat.Png);

            Process.Start(@"Mandelbrot.png");
        }

        public static Color HeatMapColor(double value, double min, double max)
        {
            double val = (value - min) / (max - min);
            val = Math.Abs(val);
            int r = Convert.ToByte(255 * val * (val * 10 % 3 / 3));
            int g = Convert.ToByte(255 * (1 - val)*(val * 10 % 3 / 3));
            int b = Convert.ToByte(255 * (val * 10 % 3 / 3 ));

            return Color.FromArgb(255, r, g, b);
        }

        private const int Width = 1920;
        private const int Height = 1080;

        private static int ColumnCompletionCounter = 0;

        private static readonly object Lock = new object();

        private static Bitmap DrawMandel()
        {
            var b = new Bitmap(Width, Height);
            
            const double xmin = -2.1;
            const double ymin = -1.3;
            const double xmax = 1;
            const double ymax = 1.3;

            const double panX = .4443d;
            const double panY = .172d;

            const double intigralX = (xmax - xmin) / Width / Zoom;
            const double intigralY = (ymax - ymin) / Height / Zoom;

            const double scrollX = (-2.2d + panX * Zoom) * (xmax - xmin) / Zoom / 2 * Height / Width;//-2d + (xmax - xmin) / Width;
            const double scrollY = (-1.5d + panY * Zoom) * (xmax - xmin) / Zoom / 2 * Height / Width;//-1.3d + (xmax - xmin) / Height / Height;

            const int iterations = 200;

            Parallel.For((long)0, Width, i =>
            {
                var x = intigralX * i + scrollX;
                for (var z = 1; z < Height; z++)
                {
                    double x1 = 0;
                    double y1 = 0;
                    var looper = 0;
                    var y = z * intigralY + scrollY;
                    while (looper < iterations && Math.Sqrt((x1 * x1) + (y1 * y1)) < 2)//774 548
                    {
                        looper++;
                        var xx = (x1 * x1) - (y1 * y1) + x;
                        y1 = 2 * x1 * y1 + y;
                        x1 = xx;
                    }

                    // Get the percent of where the looper stopped
                    //var perc = looper / (double)iterations;
                    // Use that number to set the color
                    lock (Lock)
                    {
                        b.SetPixel((int)i, z, HeatMapColor(looper, 0, iterations));
                        //b.SetPixel((int) i, z, IterationColor(looper));
                    }
                }
                ColumnCompletionCounter++;
                Console.WriteLine("Percent Complete = " + ColumnCompletionCounter + "/" + Width + " Percent = " + ColumnCompletionCounter * 100 / Width);
            });
            return b; // bq is a globally defined bitmap
            //return bq; // Draw it to the form
        }

        public static Color IterationColor(int iteration)
        {
            int r = iteration % 7 * 255 / 7;
            int g = iteration % 11 * 255 / 11;
            int b = iteration % 19 * 255 / 19;
            return Color.FromArgb(255, r, g, b);
        }
    }
}