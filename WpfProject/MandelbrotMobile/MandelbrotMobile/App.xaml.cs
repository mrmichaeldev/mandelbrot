using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace MandelbrotMobile
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();

            try
            {
                MainPage = new MandelbrotMobile.MainPage();
            }
            catch (Exception exc)
            {
                Debugger.Break();
            }
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
