using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace IotDemo.Services
{
    public sealed class GpioService : IGpioService
    {
        private readonly GpioController _controller;

        internal List<Pin> _pins = new List<Pin>();

        public int PinCount
        {
            get
            {
                return _controller.PinCount;
            }
        }

        public GpioService()
        {
            _controller = GpioController.GetDefault();
        }

        public IPin OpenPin(int pinNumber)
        {
            Pin pin = _pins.FirstOrDefault(p => p.PinNumber == pinNumber);
            if (pin == null)
            {
                var pin0 = _controller.OpenPin(pinNumber);
                pin = new Pin(this, pin0);
                _pins.Add(pin);
            }
            return pin;
        }
    }

    public class Pin : IPin
    {
        private readonly GpioPin _pin;
        private readonly GpioService _service;

        public event EventHandler<GPinEventArgs> ValueChanged;

        public Pin(GpioService service, GpioPin pin)
        {
            _service = service;
            _pin = pin;
            _pin.ValueChanged += _pin_ValueChanged;
        }

        private void _pin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            ValueChanged?.Invoke(this, new GPinEventArgs((GpioPinEdge)args.Edge));
        }

        public int PinNumber
        {
            get
            {
                return _pin.PinNumber;
            }
        }

        public TimeSpan DebounceTimeout
        {
            get { return _pin.DebounceTimeout; }
            set { _pin.DebounceTimeout = value;  }
        }

        public void Write(PinValue value)
        {
            GpioPinValue newValue = GpioPinValue.High;
            switch (value)
            {
                case PinValue.High:
                    newValue = GpioPinValue.High;
                    break;
                case PinValue.Low:
                    newValue = GpioPinValue.Low;
                    break;
            }
            _pin.Write(newValue);
        }

        public PinValue Read()
        {
            var value = _pin.Read();
            switch (value)
            {
                case GpioPinValue.High:
                    return PinValue.High;

                case GpioPinValue.Low:
                    return PinValue.Low;
            }

            throw new Exception();
        }

        public PinMode Mode
        {
            get
            {
                var mode = _pin.GetDriveMode();
                switch (mode)
                {
                    case GpioPinDriveMode.Input:
                        return PinMode.Input;

                    case GpioPinDriveMode.Output:
                        return PinMode.Output;

                    case GpioPinDriveMode.InputPullDown:
                        return PinMode.InputPullDown;

                    case GpioPinDriveMode.InputPullUp:
                        return PinMode.InputPullUp;
                }

                throw new Exception();
            }
        }

        public void SetMode(PinMode mode)
        {
            GpioPinDriveMode newMode = GpioPinDriveMode.Output;
            switch (mode)
            {
                case PinMode.Input:
                    newMode = GpioPinDriveMode.Input;
                    break;
                case PinMode.Output:
                    newMode = GpioPinDriveMode.Output;
                    break;

                case PinMode.InputPullUp:
                    newMode = GpioPinDriveMode.InputPullUp;
                    break;
                case PinMode.InputPullDown:
                    newMode = GpioPinDriveMode.InputPullDown;
                    break;
            }
            _pin.SetDriveMode(newMode);
        }

        public void Dispose()
        {
            //_pin.Dispose();
            _service._pins.Remove(this);
            ValueChanged = null;
        }
    }

    public class GPinEventArgs : EventArgs
    {
        private GpioPinEdge edge;

        public GPinEventArgs(GpioPinEdge edge)
        {
            this.edge = edge;
        }

        public GpioPinEdge Edge
        {
            get
            {
                return edge;
            }
        }
    }

    public enum GpioPinEdge
    {
        FallingEdge,
        RisingEdge
    }
}
