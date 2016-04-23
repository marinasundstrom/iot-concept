using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;
using IotDemo.Services;
using System.Collections.ObjectModel;

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
        RelayCommand sendCommand;
        bool pinState = false;
        private Pin selectedPin;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IIotClient client)
        {
            Client = client;

            Pins = new ObservableCollection<Pin>();
        }

        public async Task Initialize()
        {
            Pins.Clear();

            Pins.Add(new Pin("Port 1", 4));
            Pins.Add(new Pin("Port 2", 5));
            Pins.Add(new Pin("Port 3", 6));
            Pins.Add(new Pin("Port 4", 26));

            foreach(var pin in Pins) {
                pin.State = await Client.GetPinAsync(pin.Id);
            }
        }

        public RelayCommand SendCommand
        {
            get
            {
                return sendCommand ?? (sendCommand = new RelayCommand(async () =>
                {
                    SelectedPin.State = await Client.TogglePinAsync(SelectedPin.Id);
                }));
            }
        }

        public IIotClient Client { get; }
        internal ObservableCollection<Pin> Pins { get; private set; }

        public Pin SelectedPin
        {
            get
            {
                return selectedPin;
            }
            set
            {
                if(selectedPin != value)
                {
                    selectedPin = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}