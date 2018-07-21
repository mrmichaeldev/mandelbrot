using System.ComponentModel;

using System;
using System.Collections.Generic;
using System.Text;
using MandelbrotRenderer;
using System.IO;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp;

namespace MandelbrotMobile.Viewmodels
{
    public class MainViewmodel : INotifyPropertyChanged
    {
        const double xmin = -2.1;
        const double xmax = 1;
        const double ymin = -1.3;
        const double ymax = 1.3;

        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public double PanX { get; set; } = .4443d;
        public double PanY { get; set; } = .172d;
        public double Zoom { get; set; } = 2d;

        private int _iterations = 200;
        public string Iterations
        {
            get => _iterations.ToString();
            set
            {
                if (int.TryParse(value, out _iterations))
                {
                    Update();
                    OnPropertyChanged();
                }
            }
        }

        public event EventHandler<ImageUpdatedEventArgs> ImageUpdated;

        public void Update()
        {
            var array = RendererFactory.GetRender(_iterations, Width, Height, PanX, PanY, Zoom);
            var image = Image.LoadPixelData(array, Width, Height);
            var memoryStream = new MemoryStream();
            image.Save(memoryStream, new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder());
            ImageUpdated?.Invoke(this, new ImageUpdatedEventArgs { ImageData = memoryStream });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public class ImageUpdatedEventArgs : EventArgs
        {
            public MemoryStream ImageData { get; set; }
        }
    }
}
