using MandelbrotMobile.Viewmodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MandelbrotMobile
{
	public partial class MainPage : ContentPage
	{
        private readonly MainViewmodel _mainViewmodel = new MainViewmodel();
		public MainPage()
		{
			InitializeComponent();
            BindingContext = _mainViewmodel;
            _mainViewmodel.ImageUpdated += _mainViewmodel_ImageUpdated;
		}

        private void _mainViewmodel_ImageUpdated(object sender, MainViewmodel.ImageUpdatedEventArgs e)
        {
            //image.Source = ImageSource.FromStream(() => e.ImageData);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //_mainViewmodel.Width = (int)image.Width;
            //_mainViewmodel.Height = (int)image.Height;
            //_mainViewmodel.Update();
        }
    }
}
