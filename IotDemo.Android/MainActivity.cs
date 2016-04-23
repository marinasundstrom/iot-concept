using Android.App;
using Android.Widget;
using Android.OS;
using IotDemo.ViewModels;
using GalaSoft.MvvmLight.Helpers;
using Android.Views;
using System;
using System.Linq;

namespace IotDemo
{
	[Activity (Label = "IotDemo", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : GalaSoft.MvvmLight.Views.ActivityBase
	{
        private Binding<Pin, Pin> binding;
        private ListView listView;

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
			//Button button = FindViewById<Button> (Resource.Id.myButton);
			
			//button.SetCommand ("Click", ViewModel.SendCommand);

            listView = FindViewById<ListView>(Resource.Id.listView1);

            listView.Adapter = ViewModel.Pins.GetAdapter(GetPinView);

            listView.ItemClick += (s, e) =>
            {
                ViewModel.SelectedPin = ViewModel.Pins.ElementAt(e.Position);
                ViewModel.SendCommand.Execute(null);
            };

            //binding = this.SetBinding(() => ViewModel.SelectedPin)
            //    .WhenSourceChanges(() => listView.SetSelection(ViewModel.Pins.IndexOf(ViewModel.SelectedPin)));

            await ViewModel.Initialize ();
		}

        private View GetPinView(int position, Pin pin, View convertView)
        {
            View view = convertView ?? LayoutInflater.Inflate(Resource.Layout.ItemPin, null);

            var titleTextView = view.FindViewById<TextView>(Resource.Id.textView1);

            titleTextView.Text = pin.Title;

            pin.PropertyChanged += (s, e) =>
            {
                var p = (Pin)s;
                titleTextView.Text = GetItemTitle(p);
            };

            return view;
        }

        private static string GetItemTitle(Pin p)
        {
            return $"{p.Title} ({(p.State ? "ON" : "OFF")})";
        }
    }
}


