using System;
using System.Threading.Tasks;

namespace IotDemo.Services
{
	public class DummyIotClient : IIotClient
	{
		public async Task<bool> TogglePinAsync (int id)
		{
			return false;
		}

		public async Task<bool> GetPinAsync (int id)
		{
			return false;
		}

		public async Task ClearPinAsync (int id)
		{
			
		}
	}
}

