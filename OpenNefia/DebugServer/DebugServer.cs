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
using System.Text.RegularExpressions;

namespace OpenNefia.Core.DebugServer
{
    internal interface IDebugServer
    {
        void Startup();
        void CheckForRequests();
        void Shutdown();
    }

    /// <summary>
    /// A server for running C# code in debug builds of the game.
    /// 
    /// Clients connect via TCP and send JSON messages as a single line, where
    /// a newline indicates the message is finished. The same convention is used
    /// by the server.
    /// 
    /// This purposely avoids async to be able to access IoC things on the main 
    /// thread. It is a stop-the-world sort of setup for debugging purposes only.
    /// </summary>
    internal class DebugServer : IDebugServer
    {
#if !FULL_RELEASE
        [Dependency] private readonly IReplExecutor _replExecutor = default!;

#endif
        private TcpListener? _server;

        private readonly JsonSerializerSettings _jsonSettings = new()
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.None
        };

        public void Startup()
        {
#if !FULL_RELEASE
            // Data buffer for incoming data.  

            var ipAddress = IPAddress.Parse("127.0.0.1");
            var port = 4567;
            var endpoint = new IPEndPoint(ipAddress, port);

            try
            {
                _server = new TcpListener(endpoint);
                _server.Start();
                Logger.InfoS(CommonSawmills.DebugServer, $"Listening on {endpoint}");
            }
            catch (Exception e)
            {
                Logger.ErrorS(CommonSawmills.DebugServer, e, "Failed to start debug server!");
                _server = null;
                return;
            }
#endif
        }

        public void CheckForRequests()
        {
#if !FULL_RELEASE
            if (_server == null)
                return;

            if (!_server.Pending())
                return;
            
            try
            {
                using (var client = _server.AcceptTcpClient())
                {
                    Logger.DebugS(CommonSawmills.DebugServer, "client connected");

                    // Get a stream object for reading and writing
                    using (var stream = client.GetStream())
                    {
                        string? json;
                        var reader = new StreamReader(stream, EncodingHelpers.UTF8);
                        // NOTE: The client must send all JSON with escaped newlines.
                        // A newline in the stream means the message is finished.
                        json = reader.ReadLine();

                        if (json == null)
                        {
                            Logger.WarningS(CommonSawmills.DebugServer, "Client did not send data");
                            return;
                        }

                        Logger.DebugS(CommonSawmills.DebugServer, $"client request: {json.Length} bytes");

                        var response = Process(json);

                        Logger.DebugS(CommonSawmills.DebugServer, $"server response: {response.Length} bytes");

                        if (stream.CanWrite)
                        {
                            using (var writer = new StreamWriter(stream, EncodingHelpers.UTF8))
                            {
                                writer.Write(response);
                            }

                        }
                        else
                        {
                            Logger.WarningS(CommonSawmills.DebugServer, "cannot send response!");
                        }
                    }

                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorS(CommonSawmills.DebugServer, ex, "Failed to connect to debug server client");
            }

#endif
}

        public void Shutdown()
        {
#if !FULL_RELEASE
            _server?.Stop();
            _server = null;
#endif
        }

        #region Processing

        private interface ICommandResult
        {
            bool Success { get; }
        }

        private class SerializedError : ICommandResult
        {
            public bool Success => false;

            public string Message { get; set; }

            public SerializedError(Exception ex)
            {
                Message = ex.ToString();
            }

            public SerializedError(string message)
            {
                Message = message;
            }
        }

        // JSON request -> JSON response
        private string Process(string json)
        {
            string responseString;
            ICommandResult commandResult;
          
            try
            {
                commandResult = RunCommand(json);
            }
            catch (Exception ex)
            {
                commandResult = new SerializedError(ex);
            }
          
            return JsonConvert.SerializeObject(commandResult, _jsonSettings);

        }

        private ICommandResult RunCommand(string json)
        {
            // FIXME: tagged unions please...
            var jobject = JObject.Parse(json);
            var command = jobject["command"];
            if (command == null)
            {
                throw new InvalidOperationException("No command specified.");
            }

            var args = jobject["args"];
            if (args == null)
            {
                throw new InvalidOperationException("No args specified.");
            }

            var commandStr = command.ToObject<string>();
            var argsStr = args.ToString();

            switch (commandStr)
            {
                case CommandExecOptions.Name:
                    var opts = JsonConvert.DeserializeObject<CommandExecOptions>(argsStr, _jsonSettings)!;
                    return CommandExec(opts);
                default:
                    throw new InvalidOperationException($"Invalid command '{commandStr}'.");
            }
        }

        #endregion

        #region Commands

        private class CommandExecOptions
        {
            public const string Name = "exec";

            public string Script { get; set; } = string.Empty;
        }
        private class CommandExecResult : ICommandResult
        {
            public bool Success => true;

            public string Result { get; set; }

            public CommandExecResult(string result)
            {
                Result = result;
            }
        }

        private CommandExecResult CommandExec(CommandExecOptions opts)
        {
            _replExecutor.Initialize();
            var script = Regex.Unescape(opts.Script);
            var result = _replExecutor.Execute(script);


            switch (result)
            {
                case ReplExecutionResult.Success success:
                    Logger.InfoS(CommonSawmills.DebugServer, $"Exec result: {success.Result}");
                    return new CommandExecResult(success.Result);
                case ReplExecutionResult.Error err:
                    Logger.WarningS(CommonSawmills.DebugServer, $"Exec error: {err.Exception.Message}");
                    throw err.Exception;
                default:
                    throw new InvalidOperationException("Exec failed");
            }
        }

        #endregion
    }
}
