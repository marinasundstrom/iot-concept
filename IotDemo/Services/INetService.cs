using IotDemo.Utils;
using System;

namespace IotDemo.Services
{
    public interface INetService
    {
        void Start();
        void Stop();

        event EventHandler<PinEventArgs> PinChanged;
    }

    public class PinEventArgs : EventArgs
    {
        public PinEventArgs(HttpServerRequest request, IPin pin)
        {
            Request = request;
            Pin = pin;
        }

        public IPin Pin { get; private set; }

        public HttpServerRequest Request { get; private set; }
    }
}