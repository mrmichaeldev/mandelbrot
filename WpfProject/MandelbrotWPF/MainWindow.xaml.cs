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
using MandelbrotWPF.Viewmodels;
using static MandelbrotWPF.Viewmodels.MainViewmodel;

namespace MandelbrotWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewmodel mainViewmodel = new MainViewmodel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mainViewmodel;
            mainViewmodel.ImageUpdated += MainViewmodel_ImageUpdated;

            Loaded += MainWindow_Loaded;
        }

        private void MainViewmodel_ImageUpdated(object sender, ImageUpdatedEventArgs e)
        {
            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.StreamSource = e.ImageData;
            bitmapImage.EndInit();
            Image.Source = bitmapImage;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            mainViewmodel.Width = (int)this.Width;
            mainViewmodel.Height = (int)this.Height;
            mainViewmodel.Update();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            var moveSpeed = 4d;
            base.OnKeyUp(e);

            switch (e.Key)
            {
                case Key.OemMinus:
                    {
                        mainViewmodel.Zoom *= 1 - 1 / moveSpeed;
                        mainViewmodel.Update();
                        break;
                    }
                case Key.OemPlus:
                    {
                        mainViewmodel.Zoom *= 1 + 1 / moveSpeed;
                        mainViewmodel.Update();
                        break;
                    }
                case Key.Left:
                    {
                        mainViewmodel.PanY -= 2.6/mainViewmodel.Zoom / moveSpeed;
                        mainViewmodel.Update();
                        break;
                    }
                case Key.Right:
                    {
                        mainViewmodel.PanY += 2.6 / mainViewmodel.Zoom / moveSpeed;
                        mainViewmodel.Update();
                        break;
                    }
                case Key.Up:
                    {
                        mainViewmodel.PanX -= 3.1 / mainViewmodel.Zoom/ moveSpeed;
                        mainViewmodel.Update();
                        break;
                    }
                case Key.Down:
                    {
                        mainViewmodel.PanX += 3.1 / mainViewmodel.Zoom / moveSpeed;
                        mainViewmodel.Update();
                        break;
                    }
            }
        }
    }
}
