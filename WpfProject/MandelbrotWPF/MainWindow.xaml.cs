using MandelbrotWPF.Bitmaptemplate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
//using System.Drawing;
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
using Color = MandelbrotWPF.Bitmaptemplate.Color;

namespace MandelbrotWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _imagePath = string.Empty;
        public string ImagePath { get { return _imagePath; } }
        private BitmapTemplate CurrentBitmap = new BitmapTemplate{ Width = Width, Height = Height, Image = new Color[Width, Height] };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //var file = File.Create(@"Mandelbrot.png");

            //var bitmap = new Bitmap(1, 1);
            //bitmap.Save(file, ImageFormat.Png);
            Update();
            //Process.Start(@"Mandelbrot.png");
        }

        protected async override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            switch (e.Key)
            {
                case Key.Down:
                    {
                        var timestamp = DateTime.Now;
                        var mandelXForm = (int)((double)CurrentBitmap.Width / PercentOffset);
                        scrollY -= intigralY * PercentOffset / 100;
                        for (int y = mandelXForm; y < CurrentBitmap.Height; y++)//foreach row
                        {
                            for (int x = 0; x < CurrentBitmap.Width; x++)
                            {
                                CurrentBitmap.Image[x, y - mandelXForm] = CurrentBitmap.Image[x, y];
                            }
                        }
                        var shift = DateTime.Now - timestamp;
                        Console.WriteLine("Shift seconds "+ shift.TotalSeconds);

                        timestamp = DateTime.Now;
                        await RedrawBitmap(FromTemplate(CurrentBitmap));
                        var shiftRedraw = DateTime.Now - timestamp;
                        Console.WriteLine("Shift redraw " + shiftRedraw.TotalSeconds);

                        timestamp = DateTime.Now;
                        for (int y = CurrentBitmap.Height - mandelXForm; y < CurrentBitmap.Height; y++)
                        {
                            for (int x = 0; x < CurrentBitmap.Width; x++)
                            {
                                CurrentBitmap.Image[x, y].ShouldNotCalculate = false;
                                CurrentBitmap.Image[x, y].R = 0;
                                CurrentBitmap.Image[x, y].G = 0;
                                CurrentBitmap.Image[x, y].B = 0;
                            }
                        }

                        var reset = DateTime.Now - timestamp;
                        Console.WriteLine("Reset " + reset.TotalSeconds);
                        timestamp = DateTime.Now;

                        await RedrawBitmap(FromTemplate(CurrentBitmap));

                        var redraw = DateTime.Now - reset;
                        Console.WriteLine("Redraw " + redraw);
                        
                        break;
                    }
            }
            Update();
        }

        private const double PercentOffset = 10;

        public async void Update()
        {
            //var file = File.Create(@"Mandelbrot.png");
            //bitmap.Save(file, ImageFormat.Png);

            var bitmap = FromTemplate( await DrawMandel(CurrentBitmap));



            //_imagePath = @"Mandelbrot.png";
        }

        public async Task RedrawBitmap(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                this.Dispatcher.Invoke(() =>
                    {
                        Image.Source = bitmapImage;
                    });
            }
        }

        public Bitmap FromTemplate(BitmapTemplate template)
        {
            var bitmap = new Bitmap(template.Width, template.Height);
            for (int x = 0; x < template.Width; x++)
            {
                for (int y = 0; y < template.Height; y++)
                {
                    var color = template.Image[x, y];
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(color.R, color.G, color.B));
                }
            }
            return bitmap;
        }

        public Color HeatMapColor(double value, double min, double max)
        {
            double val = (value - min) / (max - min);
            val = Math.Abs(val);
            byte r = Convert.ToByte(255 * val);
            byte g = Convert.ToByte(255 * (1 - val));
            byte b = Convert.ToByte(255 * (val * 1000 % 100 / 1000));
            //byte b = Convert.ToByte(255 * SpectrumSmooth(max, value)/max);

            return new Color { R = r, B = b, G = g, ShouldNotCalculate = true };
            //return Color.FromArgb(255, r, g, b);
        }

        public double SpectrumSmooth(double value, double max)
        {
            //nsmooth := n + 1 - Math.log(Math.log(zn.abs()))/Math.log(2)
            return max + 1 - Math.Log(Math.Log(Math.Abs(value))) * Math.Log(2);
        }

        //TODO: change to size of window
        private const int Width = 1920;
        private const int Height = 1080;

        private int ColumnCompletionCounter = 0;

        private readonly object Lock = new object();
        private readonly object OuterLock = new object();

        const double xmin = -2.1;
        const double ymin = -1.3;
        const double xmax = 1;
        const double ymax = 1.3;
        private const double Zoom = 2d;
        const double intigralX = (xmax - xmin) / Width / Zoom;
        const double intigralY = (ymax - ymin) / Height / Zoom;

        double scrollX = -2d;// + (xmax - xmin) / Width / Zoom;
        double scrollY = -1.3d;// + (ymax - ymin) / Height / Zoom;

        const int iterations = 50;

        private async Task<BitmapTemplate> DrawMandel(BitmapTemplate source)
        {
            Parallel.For((long) 0, Width, i =>
            {
                var x = intigralX*i + scrollX;
                for (var z = 1; z < Height; z++)
                {
                    if (source.Image[i, z].ShouldNotCalculate ) continue;
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
                        source.Image[i,z] = HeatMapColor(looper, 0, iterations);
                        //source.SetPixel((int) i, z, HeatMapColor(looper, 0, iterations));
                        //source.SetPixel((int)i, z, IterationColor(looper, looper, 0, iterations));
                        //b.SetPixel((int) i, z, IterationColor(looper));
                    }
                }
                Console.WriteLine("Percent Complete = " + ColumnCompletionCounter + "/" + Width + " Percent = " +
                                    ColumnCompletionCounter*100/Width);
            });
            return source;
        }

        public Color IterationColor(int iteration, double value, double min, double max)
        {
            //byte r = Convert.ToByte(iteration % 7 * 255 / 7);
            //byte g = Convert.ToByte(iteration % 11 * 255 / 11);
            //byte b = Convert.ToByte(iteration % 19 * 255 / 19);

            double val = (value - min) / (max - min);
            val = Math.Abs(val);

            byte b = Convert.ToByte(iteration % 511 * 255 / 511);
            byte g = Convert.ToByte(iteration % 113 * 255 / 113);
            byte r = Convert.ToByte(255 * val);

            return new Color { R = r, G = g, B = b };
        }
    }
}
