using System;
using Windows.Devices.Gpio;

namespace IotDemo.Services
{
    public interface IGpioService
    {
        IPin OpenPin(int pinNumber);

        int PinCount { get; }
    }

    public interface IPin : IDisposable
    {
        PinMode Mode { get; }
        int PinNumber { get; }
        PinValue Read();
        void SetMode(PinMode mode);
        void Write(PinValue value);

        event EventHandler ValueChanged;
    }

    public enum PinValue
    {
        Low,
        High
    }

    public enum PinMode
    {
        Input,
        Output,
        InputPullUp,
        InputPullDown
    }
}