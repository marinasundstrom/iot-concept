using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace IotDemo.Services
{
	public class IotClient : IIotClient
	{
		HttpClient client;

		public IotClient ()
		{
			client = new HttpClient();
			client.BaseAddress = new Uri("http://192.168.1.101:8083/", UriKind.Absolute);
		}

		public async Task<bool> GetPinAsync(int id) 
		{
			var response = await client.GetAsync(
				new Uri("/led/" + id, UriKind.Relative));

			return await ParseResponse (response);
		}

		public async Task<bool> TogglePinAsync(int id) 
		{
			var str = "{ \"state\": \"toggle\" }";
			var response = await client.PostAsync(
				new Uri("/led/" + id, UriKind.Relative), 
				new StringContent(str, Encoding.UTF8, "application/json"));
			return await ParseResponse (response);
		}

		public async Task ClearPinAsync(int id) 
		{
			var response = await client.DeleteAsync(
				new Uri("/led/" + id, UriKind.Relative));

		}

		static async Task<bool> ParseResponse (HttpResponseMessage response)
		{
			var resultStr = await response.Content.ReadAsStringAsync ();
			var obj = JObject.Parse (resultStr.Replace ("True", "true").Replace ("False", "false"));
			var pinId = obj.Value<int> ("id");
			var state = obj.Value<bool> ("state");
			return state;
		}
	}
}

