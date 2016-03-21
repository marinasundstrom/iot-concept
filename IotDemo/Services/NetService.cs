using IotDemo.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IotDemo.Services
{
    public sealed class NetService : INetService
    {
        private HttpServer server;
        private Task task;
        private CancellationTokenSource tokenSource;

        public event EventHandler<PinEventArgs> PinChanged;

        public IGpioService GpioService { get; private set; }

        public NetService(IGpioService gpioService)
        {
            this.GpioService = gpioService;
        }

        public void Start()
        {
            server = new HttpServer(8083);

            tokenSource = new CancellationTokenSource();
            CancellationToken ct = tokenSource.Token;

            task = Task.Run(async () =>
            {
                while (server.IsListening)
                {
                    try
                    {
                        var context = await server.GetContextAsync();
                        await ProcessRequest(context);
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc.ToString());
                    }
                }
            }, ct);
        }

        private async Task ProcessRequest(HttpServerContext context)
        {
            try
            {
                var request = context.Request;

                var parts = request.Url.LocalPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    return;

                if (parts.Length == 2)
                {
                    if (parts[0] == "led")
                    {
                        var id = int.Parse(parts[1]);

                        var pin = GpioService.OpenPin(id);

                        switch (context.Request.HttpMethod)
                        {
                            case HttpMethod.Get:
                                await GetPin(context, pin);
                                break;

                            case HttpMethod.Post:
                            case HttpMethod.Put:
                                await UpdatePin(context, pin);
                                break;

                            case HttpMethod.Delete:
                                await ClearPin(context, pin);
                                break;

                            default:
                                MethodNotAllowed(context);
                                break;
                        }

                        return;
                    }
                }

                InternalServerError(context);
            }
            catch (Exception)
            {
                InternalServerError(context);
            }
        }

        private static void InternalServerError(HttpServerContext context)
        {
            using (var response = context.Response)
            {
                response.InternalServerError();
            }
        }

        private static void MethodNotAllowed(HttpServerContext context)
        {
            using (var response = context.Response)
            {
                response.MethodNotAllowed();
            }
        }

        private async Task ClearPin(HttpServerContext context, IPin pin)
        {
            bool currentState = false;
            pin.Write(currentState ? PinValue.High : PinValue.Low);

            using (var response = context.Response)
            {
                await RespondPinValue(context, pin.PinNumber, currentState);
            }

            PinChanged?.Invoke(this, new PinEventArgs(context.Request, pin));
        }

        private async Task UpdatePin(HttpServerContext context, IPin pin)
        {
            var request = context.Request;
            var inputStream = request.InputStream;
            var currentState = pin.Read() == PinValue.High;
            using (var streamReader = new StreamReader(inputStream.AsStreamForRead()))
            {
                var content = await streamReader.ReadToEndAsync();
                var obj = JObject.Parse(content);
                JToken token = null;
                if (obj.TryGetValue("state", out token))
                {
                    switch (token.Type)
                    {
                        case JTokenType.Boolean:
                            currentState = token.Value<bool>();
                            break;

                        case JTokenType.String:
                            var str = token.Value<string>();
                            if (str == "toggle")
                            {
                                currentState = !currentState;
                            }
                            break;
                    }
                }
            }

            pin.Write(currentState ? PinValue.High : PinValue.Low);

            using (var response = context.Response)
            {
                await RespondPinValue(context, pin.PinNumber, currentState);
            }

            PinChanged?.Invoke(this, new PinEventArgs(request, pin));
        }

        private static async Task GetPin(HttpServerContext context, IPin pin)
        {
            var currentState = pin.Read() == PinValue.High;
            using (var response = context.Response)
            {
                await RespondPinValue(context, pin.PinNumber, currentState);
            }
        }

        private static async Task RespondPinValue(HttpServerContext context, int id, bool state)
        {
            var response = context.Response;

            response.ContentType = "application/json";
            response.Headers["Location"] = context.Request.Url.ToString();

            var outputStream = response.OutputStream;
            var stream = outputStream.AsStreamForWrite();
            var streamWriter = new StreamWriter(stream) { AutoFlush = true };
            await streamWriter.WriteAsync("{ \"id\": " + id + ", \"state\": " + state + " }");
        }

        public void Stop()
        {
            tokenSource.Cancel();
            PinChanged = null;
        }
    }
}
