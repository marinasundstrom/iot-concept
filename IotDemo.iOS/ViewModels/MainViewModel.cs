using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;
using IotDemo.Services;

namespace IotDemo.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
		readonly int pin = 4;

		RelayCommand sendCommand;
		bool pinState = false;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
		public MainViewModel(IIotClient client)
        {
			Client = client;
        }

		public async Task Initialize() 
		{
			PinState = await Client.GetPinAsync(pin);
		}

		public RelayCommand SendCommand 
		{
			get
			{ 
				return sendCommand ?? (sendCommand = new RelayCommand(async () => {
					PinState = await Client.TogglePinAsync(pin);
				}));
			}
		}

		public bool PinState 
		{
			get 
			{ 
				return pinState;
			}

			set 
			{ 
				if (pinState != value) 
				{
					pinState = value;
					RaisePropertyChanged ();
				}
			}
		}

		public IIotClient Client { get; }
    }
}