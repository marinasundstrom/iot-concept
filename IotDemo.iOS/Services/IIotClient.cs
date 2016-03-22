using System;
using System.Threading.Tasks;

namespace IotDemo.Services
{
	public interface IIotClient
	{
		Task<bool> GetPinAsync(int id);

		Task<bool> TogglePinAsync(int id);

		Task ClearPinAsync(int id);
	}
}

