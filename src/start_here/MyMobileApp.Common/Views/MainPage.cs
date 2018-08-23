using System;

using Xamarin.Forms;

namespace MyMobileApp.Common
{
	public class MainPage : TabbedPage
	{
		public MainPage()
		{
			var predictPage = new NavigationPage(new PredictionDetailsPage())
			{
				Title = "Predict"
			};

			var settingsPage = new NavigationPage(new SettingsPage())
			{
				Title = "Settings"
			};

            var predictionListPage = new NavigationPage(new PredictionListPage())
            {
                Title = "Predictions"
            };

			switch(Device.RuntimePlatform)
			{
				case Device.iOS:
					predictPage.Icon = "tab_about.png";
					settingsPage.Icon = "tab_settings.png";
                    predictionListPage.Icon = "tab_about.png";
					break;
			}

			Children.Add(predictPage);
			Children.Add(settingsPage);
            Children.Add(predictionListPage);

			Title = Children[0].Title;
		}

		protected override void OnCurrentPageChanged()
		{
			base.OnCurrentPageChanged();
			Title = CurrentPage?.Title ?? string.Empty;
		}
	}
}
