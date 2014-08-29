using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Drawing.Color;

namespace MandelbrotWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string _imagePath = string.Empty;
        public string ImagePath { get { return _imagePath; } }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            var file = File.Create(@"Mandelbrot.png");
            
            var bitmap = new Bitmap(1, 1);
            bitmap.Save(file, ImageFormat.Png);
            
        }

        private const double Zoom = 100d;

        public async void Update()
        {
            var file = File.Create(@"Mandelbrot.png");
            var bitmap = await DrawMandel();
            bitmap.Save(file, ImageFormat.Png);

            //_imagePath = @"Mandelbrot.png";
        }

        public Color HeatMapColor(double value, double min, double max)
        {
            double val = (value - min) / (max - min);
            val = Math.Abs(val);
            byte r = Convert.ToByte(255 * val);
            byte g = Convert.ToByte(255 * (1 - val));
            byte b = Convert.ToByte(255 * (val * 100 % 10 / 10));

            return Color.FromArgb(255, r, g, b);
        }

        private const int Width = 1920;
        private const int Height = 1080;

        private int ColumnCompletionCounter = 0;

        private readonly object Lock = new object();

        private async Task<Bitmap> DrawMandel()
        {
            var b = new Bitmap(Width, Height);

            //const double xmin = -2.1;
            //const double ymin = -1.3;
            //const double xmax = 1;
            //const double ymax = 1.3;
            const double xmin = -2.1;
            const double ymin = -1.3;
            const double xmax = 1;
            const double ymax = 1.3;
            const double intigralX = (xmax - xmin)/Width/Zoom;
            const double intigralY = (ymax - ymin)/Height/Zoom;

            const double scrollX = -2d + (xmax - xmin)*774/Width;
            const double scrollY = -1.3d + (xmax - xmin)*548/Height;

            const int iterations = 500;

            Parallel.For((long) 0, Width, i =>
            {
                var x = intigralX*i + scrollX;
                for (var z = 1; z < Height; z++)
                {
                    double x1 = 0;
                    double y1 = 0;
                    var looper = 0;
                    var y = z*intigralY + scrollY;
                    while (looper < iterations && Math.Sqrt((x1*x1) + (y1*y1)) < 2) //774 548
                    {
                        looper++;
                        var xx = (x1*x1) - (y1*y1) + x;
                        y1 = 2*x1*y1 + y;
                        x1 = xx;
                    }

                    // Get the percent of where the looper stopped
                    //var perc = looper / (double)iterations;
                    // Use that number to set the color
                    lock (Lock)
                    {
                        b.SetPixel((int) i, z, HeatMapColor(looper, 0, iterations));
                        //b.SetPixel((int) i, z, IterationColor(looper));
                    }
                }
                ColumnCompletionCounter++;
                Console.WriteLine("Percent Complete = " + ColumnCompletionCounter + "/" + Width + " Percent = " +
                                    ColumnCompletionCounter*100/Width);
            });
            return b; // bq is a globally defined bitmap
            //return bq; // Draw it to the form
        }

        public Color IterationColor(int iteration)
        {
            byte r = Convert.ToByte(iteration % 7 * 255 / 7);
            byte g = Convert.ToByte(iteration % 11 * 255 / 11);
            byte b = Convert.ToByte(iteration % 19 * 255 / 19);
            return Color.FromArgb(255, r, g, b);
        }
    }
}
