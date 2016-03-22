using System;

using UIKit;
using System.Net.Http;
using System.Text;
using GalaSoft.MvvmLight.Helpers;
using IotDemo.ViewModels;

namespace IotDemo
{
	public partial class ViewController : GalaSoft.MvvmLight.Views.ControllerBase
	{
		public MainViewModel ViewModel
		{
			get 
			{
				return ViewModelLocator.Instance.Main;
			}
		}

		public ViewController (IntPtr handle) : base (handle)
		{
		}

		public async override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			// Perform any additional setup after loading the view, typically from a nib.

			var binding = this.SetBinding (() => ViewModel.PinState)
				.WhenSourceChanges(() => toggleButton.TitleLabel.Text = ViewModel.PinState.ToString());

			toggleButton.SetCommand("TouchUpInside", ViewModel.SendCommand);

			await ViewModel.Initialize ();
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}

