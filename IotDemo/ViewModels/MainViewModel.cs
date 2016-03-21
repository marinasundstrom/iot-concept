using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IotDemo.Services;
using IotDemo.Utils;
using System;
using System.Collections.Generic;
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

            Initialize();
        }

        private void Initialize()
        {
            pin = GpioService.OpenPin(4);
            pin.ValueChanged += Pin_PinValueChanged;

            pin.SetMode(PinMode.Out);
            pin.Write(PinValue.Low);

            NetService.PinChanged += NetService_PinChanged;
            NetService.Start();
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

        private void Pin_PinValueChanged(object sender, EventArgs e)
        {
            Dispatcher.RunAsync(() =>
            {
                var value = pin.Read() == PinValue.High;
                if (value != Pin1)
                {
                    _pin1 = value;
                    RaisePropertyChanged(() => Pin1);
                }
            });
        }

        /// <summary>
        /// The <see cref="Pin1" /> property's name.
        /// </summary>
        public const string Pin1PropertyName = "Pin1";

        private bool _pin1 = false;
        private IPin pin;

        /// <summary>
        /// Sets and gets the Pin1 property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool Pin1
        {
            get
            {
                return _pin1;
            }
            set
            {
                Set(() => Pin1, ref _pin1, value);

                Dispatcher.RunAsync(() =>
                {
                    ModifiedBy = $"Last updated from this device. ({DateTime.Now})";
                    pin.Write(value ? PinValue.High : PinValue.Low);
                });
            }
        }

        /// <summary>
        /// The <see cref="ModifiedBy" /> property's name.
        /// </summary>
        public const string ModifiedByPropertyName = "ModifiedBy";

        string _modifiedBy = string.Empty;

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
}
