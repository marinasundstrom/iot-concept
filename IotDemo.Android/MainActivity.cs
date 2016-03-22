using Android.App;
using Android.Widget;
using Android.OS;
using IotDemo.ViewModels;
using GalaSoft.MvvmLight.Helpers;

namespace IotDemo
{
	[Activity (Label = "IotDemo", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : GalaSoft.MvvmLight.Views.ActivityBase
	{
		public MainViewModel ViewModel
		{
			get 
			{
				return ViewModelLocator.Instance.Main;
			}
		}

		protected async override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button> (Resource.Id.myButton);
			
			button.SetCommand ("Click", ViewModel.SendCommand);
			this.SetBinding (() => ViewModel.PinState)
				.WhenSourceChanges (() => button.Text = ViewModel.PinState.ToString ());

			await ViewModel.Initialize ();
		}
	}
}


