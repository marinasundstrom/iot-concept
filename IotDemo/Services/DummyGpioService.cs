using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDemo.Services
{
    public sealed class DummyGpioService : IGpioService
    {
        internal List<IPin> _pins = new List<IPin>();

        public int PinCount
        {
            get
            {
                return 4;
            }
        }

        public IPin OpenPin(int pinNumber)
        {
            IPin pin = _pins.FirstOrDefault(p => p.PinNumber == pinNumber);
            if (pin == null)
            {
                pin = new DummyPin(this, pinNumber);
                _pins.Add(pin);
            }
            return pin;
        }
    }

    public class DummyPin : IPin
    {
        private PinMode _mode;
        private DummyGpioService _service;
        private PinValue _value;

        public DummyPin(DummyGpioService service, int pinNumber)
        {
            _service = service;
            PinNumber = pinNumber;
        }

        public PinMode Mode
        {
            get
            {
                return _mode;
            }
        }

        public int PinNumber
        {
            get;
            private set;
        }

        public TimeSpan DebounceTimeout
        {
            get;
            set;
        }

        public event EventHandler<GPinEventArgs> ValueChanged;

        public void Dispose()
        {
            _value = PinValue.Low;
            ValueChanged?.Invoke(this, new GPinEventArgs(GpioPinEdge.FallingEdge));
            _service._pins.Remove(this);
            ValueChanged = null;
        }

        public PinValue Read()
        {
            return _value;
        }

        public void SetMode(PinMode mode)
        {
            _mode = mode;
        }

        public void Write(PinValue value)
        {
            _value = value;
            ValueChanged?.Invoke(this, new GPinEventArgs(GpioPinEdge.FallingEdge));
        }
    }
}
