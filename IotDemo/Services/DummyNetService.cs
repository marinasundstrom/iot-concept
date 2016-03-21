using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotDemo.Services
{
    public sealed class DummyNetService : INetService
    {
        public event EventHandler<PinEventArgs> PinChanged;

        public void Start()
        {
            
        }

        public void Stop()
        {
            
        }
    }
}
