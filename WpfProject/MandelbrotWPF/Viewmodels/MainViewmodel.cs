using MandelbrotRenderer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace MandelbrotWPF.Viewmodels
{
    public class MainViewmodel : INotifyPropertyChanged
    {
        public event EventHandler<ImageUpdatedEventArgs> ImageUpdated;

        public void Update()
        {
            var width = 1920;
            var height = 1080;
            var array = RendererFactory.GetRender(@"Mandelbrot.png", width, height);
            var image = Image.LoadPixelData(array, width, height);
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
