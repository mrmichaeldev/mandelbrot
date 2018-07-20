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
            mainViewmodel.Update();
        }

        protected async override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            switch (e.Key)
            {
                case Key.Down:
                    {
                        mainViewmodel.Update();
                        break;
                    }
            }
        }
    }
}
