using IotDemo.Utils;
using System;
using System.Net.Http;

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
        public PinEventArgs(HttpListenerRequest request, IPin pin)
        {
            Request = request;
            Pin = pin;
        }

        public IPin Pin { get; private set; }

        public HttpListenerRequest Request { get; private set; }
    }
}