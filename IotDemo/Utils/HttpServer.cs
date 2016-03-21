using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace IotDemo.Utils
{
    /// <summary>
    /// Extended and reshaped from code found at http://www.dzhang.com/blog/2012/09/18/a-simple-in-process-http-server-for-windows-8-metro-apps
    /// </summary>
    public class HttpServer : IDisposable
    {
        private const uint BufferSize = 8192;

        private readonly StreamSocketListener listener;
        private readonly Queue<TaskCompletionSource<HttpServerContext>> queue = new Queue<TaskCompletionSource<HttpServerContext>>();

        public bool IsListening { get; internal set; }

        public HttpServer(int port)
        {
            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
            this.listener.BindServiceNameAsync(port.ToString());

            IsListening = true;
        }

        public void Dispose()
        {
            this.listener.Dispose();
            IsListening = false;
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            var tcs = queue.Peek();

            if (tcs != null)
            {
                try
                {
                    var input = socket.InputStream;

                    // this works for text only
                    var request = new StringBuilder();
                    byte[] data = new byte[BufferSize];
                    IBuffer buffer = data.AsBuffer();
                    uint dataRead = BufferSize;
                    while (dataRead == BufferSize)
                    {
                        await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                        request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                        dataRead = buffer.Length;
                    }

                    var localEndpoint = new IPEndPoint(IPAddress.Parse(socket.Information.LocalAddress.ToString()), int.Parse(socket.Information.LocalPort));
                    var remoteEndpoint = new IPEndPoint(IPAddress.Parse(socket.Information.RemoteAddress.ToString()), int.Parse(socket.Information.RemotePort));

                    var requestLines = request.ToString().Split('\n');
                    string requestMethod = requestLines[0].TrimEnd('\r');
                    string[] requestParts = requestMethod.Split(' ');

                    var headers = ParseHeaders(requestLines);

                    var url = (new UriBuilder(headers["Host"].ToString() + requestParts[1])).Uri;
                    var httpMethod = (HttpMethod)Enum.Parse(typeof(HttpMethod), requestParts[0], true);

                    var d = new StringBuilder();
                    requestLines = requestLines.SkipWhile(x => x != "\r").Skip(1).Select(x => x.TrimEnd('\0')).ToArray();
                    foreach (var line in requestLines)
                    {
                        d.Append(line);
                    }
                    var content = d.ToString();
                    var inputStream = new InMemoryRandomAccessStream();
                    var is2 = inputStream.AsStreamForWrite();
                    var bodyBuffer = Encoding.UTF8.GetBytes(content);
                    await is2.WriteAsync(bodyBuffer, 0, bodyBuffer.Length);
                    await is2.FlushAsync();
                    is2.Seek(0, SeekOrigin.Begin);

                    var requestObj = new HttpServerRequest(localEndpoint, remoteEndpoint, url, requestParts[2], httpMethod, headers, is2.AsInputStream());
                    var responseObj = new HttpServerResponse(socket, requestObj);

                    var context = new HttpServerContext(requestObj, responseObj);

                    tcs.SetResult(context);
                }
                catch (Exception exc)
                {
                    tcs.SetException(exc);
                }
                finally
                {
                    queue.Dequeue();
                }
            }
        }

        private static Dictionary<string, object> ParseHeaders(string[] requestLines)
        {
            var headers = new Dictionary<string, object>();

            var headerLines = requestLines.Skip(1).TakeWhile(x => x != "\r").ToArray();
            foreach (var headerLine in headerLines)
            {
                var parts = headerLine.TrimEnd('\r').Split(':');
                headers.Add(parts[0], parts[1].Trim());
            }

            return headers;
        }

        public Task<HttpServerContext> GetContextAsync()
        {
            var tcs = new TaskCompletionSource<HttpServerContext>();
            queue.Enqueue(tcs);
            return tcs.Task;
        }
    }

    public class HttpServerContext
    {
        internal HttpServerContext(HttpServerRequest request, HttpServerResponse response)
        {
            Request = request;
            Response = response;
        }

        public HttpServerRequest Request { get; private set; }

        public HttpServerResponse Response { get; private set; }
    }

    public class HttpServerRequest
    {
        internal HttpServerRequest(IPEndPoint localEndpoint, IPEndPoint remoteEndpoint, Uri url, string protocolVersion, HttpMethod httpMethod, IDictionary<string, object> headers, IInputStream inputStream)
        {
            LocalEndpoint = localEndpoint;
            RemoteEndpoint = remoteEndpoint;

            Url = url;
            ProtocolVersion = protocolVersion;
            HttpMethod = httpMethod;
            Headers = headers;
            InputStream = inputStream;
        }

        public IPEndPoint LocalEndpoint { get; private set; }

        public IPEndPoint RemoteEndpoint { get; private set; }

        public Uri Url { get; private set; }

        public HttpMethod HttpMethod { get; private set; }

        public IDictionary<string, object> Headers { get; private set; }

        public IInputStream InputStream { get; private set; }

        public string ProtocolVersion { get; private set; }

        public string Host
        {
            get
            {
                return Headers["Host"].ToString();
            }
        }

        public bool KeepAlive
        {
            get
            {
                return Headers["Connection"].ToString() == "keep-alive";
            }
        }

        public string UserAgent
        {
            get
            {
                return Headers["User-Agent"].ToString();
            }
        }

        public string Accept
        {
            get
            {
                return Headers["Accept"].ToString();
            }
        }

        public string AcceptEncoding
        {
            get
            {
                return Headers["Accept-Encoding"].ToString();
            }
        }

        public string AcceptLanguage
        {
            get
            {
                return Headers["Accept-Language"].ToString();
            }
        }

        public string ContentType
        {
            get
            {
                return Headers["Content-Type"].ToString();
            }
        }

        public int ContentLength
        {
            get
            {
                return int.Parse(Headers["Content-Length"].ToString());
            }
        }
    }

    public class HttpServerResponse : IDisposable
    {
        private StreamSocket Socket;

        internal HttpServerResponse(StreamSocket socket, HttpServerRequest request)
        {
            this.Socket = socket;

            Request = request;
            Headers = new Dictionary<string, object>();

            OutputStream = new InMemoryRandomAccessStream();

            ProtocolVersion = request.ProtocolVersion;
            StatusCode = 200;
            StatusDescription = "OK";
        }

        public HttpServerRequest Request { get; private set; }

        public IDictionary<string, object> Headers { get; private set; }

        public IOutputStream OutputStream { get; private set; }

        public string ProtocolVersion { get; private set; }

        public int StatusCode { get; set; }

        public string StatusDescription { get; set; }

        public bool KeepAlive
        {
            get
            {
                return Headers["Connection"].ToString() == "keep-alive";
            }

            set
            {
                if (value)
                {
                    Headers["Connection"] = "keep-alive";

                }
                else
                {
                    Headers["Connection"] = "close";
                }
            }
        }

        public string ContentType
        {
            get
            {
                return Headers["Content-Type"].ToString();
            }

            set
            {
                Headers["Content-Type"] = value;
            }
        }

        public Uri RedirectLocation
        {
            get
            {
                return new Uri(Headers["Location"].ToString());
            }

            set
            {
                Headers["Location"] = value.ToString();
            }
        }

        public long ContentLength
        {
            get
            {
                return this.Socket.OutputStream.AsStreamForWrite().Length;
            }
        }

        private string MakeHeaders()
        {
            var sb = new StringBuilder();
            foreach (var header in Headers)
            {
                sb.Append($"{header.Key}: {header.Value}\r\n");
            }
            return sb.ToString();
        }

        private async Task SendMessage()
        {
            var outputStream = OutputStream as InMemoryRandomAccessStream;
            outputStream.Seek(0);

            var outputStream2 = Socket.OutputStream;

            var socketStream = outputStream2.AsStreamForWrite();
            string header = $"{ProtocolVersion} {StatusCode} {StatusDescription}\r\n" +
                            MakeHeaders() +
                            $"Content-Length: {outputStream.Size}\r\n" +
                            "\r\n";

            byte[] headerArray = Encoding.UTF8.GetBytes(header);
            await socketStream.WriteAsync(headerArray, 0, headerArray.Length);
            await outputStream.AsStreamForRead().CopyToAsync(socketStream);

            await socketStream.FlushAsync();

        }

        public async void Dispose()
        {
            if (Socket != null)
            {
                await CloseAsync();
            }
            else
            {
                throw new InvalidOperationException("Connection is already disposed.");
            }
        }

        public async Task CloseAsync()
        {
            if (Socket != null)
            {
                await SendMessage();

                Socket.Dispose();
                Socket = null;
            }
            else
            {
                throw new InvalidOperationException("Connection is already closed.");
            }
        }

        public async Task Redirect(Uri location)
        {
            using (var outputStream = Socket.OutputStream)
            {
                using (var socketStream = outputStream.AsStreamForWrite())
                {
                    StatusCode = 301;
                    StatusDescription = "Moved permanently";

                    string header = $"{ProtocolVersion} {StatusCode} {StatusDescription}\r\n" +
                                    $"Location: {RedirectLocation}" +
                                    $"Content-Length: 0\r\n" +
                                    "Connection: close\r\n" +
                                    "\r\n";

                    byte[] headerArray = Encoding.UTF8.GetBytes(header);
                    await socketStream.WriteAsync(headerArray, 0, headerArray.Length);
                    await socketStream.FlushAsync();

                    Socket.Dispose();
                    Socket = null;
                }
            }
        }
    }

    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Patch,
        Delete
    }
}
