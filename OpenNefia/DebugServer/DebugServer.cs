using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Net.Sockets;

namespace OpenNefia.Core.DebugServer
{
    internal interface IDebugServer
    {
        void Startup();
        void CheckForRequests();
        void Shutdown();
    }

    internal class DebugServer : IDebugServer
    {
#if !FULL_RELEASE
        [Dependency] private readonly IReplExecutor _replExecutor = default!;
        [Dependency] private readonly ITaskManager taskManager = default!;

        private HttpListener? _httpListener;
#endif

        private readonly JsonSerializerSettings _jsonSettings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        public void Startup()
        {
#if !FULL_RELEASE
            if (!HttpListener.IsSupported)
            {
                Logger.ErrorS("debugserver", $"{nameof(HttpListener)} not supported!");
                return;
            }

            var port = 4567;

            // Create a listener.
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://localhost:{port}/");
            _httpListener.Start();
            Logger.InfoS("debugserver", $"Debug server started on port {port}");
#endif
        }

        public void CheckForRequests()
        {
#if !FULL_RELEASE
            if (_httpListener == null)
                return;

            _httpListener.GetContextAsync();
            var context = _httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), _httpListener);
            bool success = context.AsyncWaitHandle.WaitOne(0, true);
#endif
        }

        public void Shutdown()
        {
#if !FULL_RELEASE
            _httpListener?.Stop();
#endif
        }

        private void Dood()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                while (true)
                {
                    listener.Blocking = false;
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    var data = "";

                    // An incoming connection needs to be processed.  
                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        data += EncodingHelpers.UTF8.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    // Show the data on the console.  
                    Console.WriteLine("Text received : {0}", data);

                    // Echo the data back to the client.  
                    byte[] msg = EncodingHelpers.UTF8.GetBytes(data);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        private void ListenerCallback(IAsyncResult result)
        {
            var listener = (HttpListener)result.AsyncState!;

            // Use EndGetContext to complete the asynchronous operation.
            var context = listener.EndGetContext(result);
            var request = context.Request;

            var input = request.InputStream.CopyToArray();
            var json = EncodingHelpers.UTF8.GetString(input);
            request.InputStream.Close();

            // Get response object.
            var response = context.Response;
            response.ContentType = "application/json";
            response.KeepAlive = false;

            string responseString;
            ICommandResult commandResult;

            if (request.HttpMethod == HttpMethod.Post.Method)
            {
                try
                {
                    commandResult = Process(json);
                }
                catch (Exception ex)
                {
                    commandResult = new SerializedError(ex);
                    response.StatusCode = 400;
                }
            }
            else
            {
                commandResult = new SerializedError("Not found");
                response.StatusCode = 400;
            }

            response.StatusCode = (int)(commandResult.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
            responseString = JsonConvert.SerializeObject(commandResult, _jsonSettings);

            // Construct a response. 
            var buffer = EncodingHelpers.UTF8.GetBytes(responseString);

            // Write to response stream.
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // Close the output stream.
            output.Close();
        }

        private interface ICommandResult
        {
            bool Success { get; }
        }

        private class SerializedError : ICommandResult
        {
            public bool Success => false;

            public string Message { get; }

            public SerializedError(Exception ex)
            {
                Message = ex.ToString();
            }

            public SerializedError(string message)
            {
                Message = message;
            }
        }

        private ICommandResult Process(string json)
        {
            // FIXME: tagged unions please...
            var jobject = JObject.Parse(json);
            var command = jobject["command"];
            if (command == null)
            {
                throw new InvalidOperationException("No command specified.");
            }

            var commandStr = command.ToObject<string>();

            switch (commandStr)
            {
                case CommandExecOptions.Name:
                    var opts = JsonConvert.DeserializeObject<CommandExecOptions>(json, _jsonSettings)!;
                    return CommandExec(opts);
                default:
                    throw new InvalidOperationException($"Invalid command '{commandStr}'.");
            }
        }

        private class CommandExecOptions
        {
            public const string Name = "exec";

            public string Command { get; } = default!;

            public string Script { get; } = default!;
        }
        private class CommandExecResult : ICommandResult
        {
            public bool Success => true;

            public string Result { get; }

            public CommandExecResult(string result)
            {
                Result = result;
            }
        }

        private CommandExecResult CommandExec(CommandExecOptions opts)
        {
            _replExecutor.Initialize();
            var result = _replExecutor.Execute(opts.Script);

            switch (result)
            {
                case ReplExecutionResult.Success success:
                    return new CommandExecResult(success.Result);
                case ReplExecutionResult.Error err:
                    throw err.Exception;
                default:
                    throw new InvalidOperationException("Exec failed");
            }
        }
    }
}
