using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IotDemo.Services;
using IotDemo.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDemo.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(INetService netService, IGpioService gpioService)
        {
            this.GpioService = gpioService;
            this.NetService = netService;

            Pins = new ObservableCollection<PinViewModel>();

            Initialize();
        }

        private void Initialize()
        {
            Pins.Add(new PinViewModel("Port 1", GpioService.OpenPin(4)));
            Pins.Add(new PinViewModel("Port 2", GpioService.OpenPin(5)));
            Pins.Add(new PinViewModel("Port 3", GpioService.OpenPin(6)));
            Pins.Add(new PinViewModel("Port 4", GpioService.OpenPin(26)));

            SetupResetPin();

            NetService.PinChanged += NetService_PinChanged;
            NetService.Start();
        }

        private void SetupResetPin()
        {
            resetPin = GpioService.OpenPin(21);
            resetPin.SetMode(PinMode.Input);
            resetPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            resetPin.ValueChanged += ResetPin_ValueChanged;
        }

        private void ResetPin_ValueChanged(object sender, GPinEventArgs e)
        {
            Dispatcher.RunAsync(() =>
            {
                if (e.Edge == GpioPinEdge.FallingEdge)
                {
                    if (Pins.All(x => !x.Value))
                    {
                        foreach (var pin in Pins)
                        {
                            pin.Value = true;
                        }
                    }
                    else
                    {
                        foreach (var pin in Pins)
                        {
                            pin.Reset();
                        }
                    }
                }
                else
                {
                    
                }
            });
        }

        private void NetService_PinChanged(object sender, PinEventArgs e)
        {
            Dispatcher.RunAsync(() =>
            {
                ModifiedBy = $"Last updated from {e.Request.RemoteEndpoint}. ({DateTime.Now})";
            });
        }

        ~MainViewModel()
        {
            NetService.Stop();
        }

        public ObservableCollection<PinViewModel> Pins { get; private set; }

        /// <summary>
        /// The <see cref="ModifiedBy" /> property's name.
        /// </summary>
        public const string ModifiedByPropertyName = "ModifiedBy";

        string _modifiedBy = string.Empty;
        private IPin resetPin;

        /// <summary>
        /// Sets and gets the ModifiedBy property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ModifiedBy
        {
            get
            {
                return _modifiedBy;
            }

            set
            {
                if (_modifiedBy == value)
                {
                    return;
                }

                _modifiedBy = value;
                RaisePropertyChanged(() => ModifiedBy);
            }
        }

        public IGpioService GpioService { get; private set; }
        public INetService NetService { get; private set; }
    }

    public class PinViewModel : ViewModelBase
    {
        IPin pin;

        public PinViewModel(string title, IPin pin)
        {
            this.Title = title;
            this.pin = pin;

            this.pin.ValueChanged += Pin_PinValueChanged;

            this.pin.SetMode(PinMode.Output);
            this.pin.Write(PinValue.Low);

            Value = this.pin.Read() == PinValue.High;
        }

        private void Pin_PinValueChanged(object sender, GPinEventArgs e)
        {
            Dispatcher.RunAsync(() =>
            {
                var value = pin.Read() == PinValue.High;
                if (value != Value)
                {
                    _value = value;
                    RaisePropertyChanged(() => Value);
                }
            });
        }

        public void Reset()
        {
            Value = false;
        }

        /// <summary>
        /// The <see cref="Value" /> property's name.
        /// </summary>
        public const string ValuePropertyName = "Value";

        private bool _value = false;

        /// <summary>
        /// Sets and gets the Pin1 property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Value
        {
            get
            {
                return _value;
            }
            set
            {
                Set(() => Value, ref _value, value);

                Dispatcher.RunAsync(() =>
                {
                    //ModifiedBy = $"Last updated from this device. ({DateTime.Now})";
                    pin.Write(value ? PinValue.High : PinValue.Low);
                });
            }
        }

        public string Title { get; private set; }
    }
}
