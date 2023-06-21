using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Logging;
using Newtonsoft.Json.Linq;

namespace Grpc.Client
{
    internal class GrpcCoreClient : GrpcClientBase
    {
        private readonly ITokenProvider _tokenProvider;

        static GrpcCoreClient()
        {
            // switch to use native DNS resolver (by default GRPC uses c-ares resolver)
            Environment.SetEnvironmentVariable("GRPC_DNS_RESOLVER", "native");
            Environment.SetEnvironmentVariable("GRPC_TRACE", "all");
            Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");

            var replace = new Regex(@"[0-9]+:[0-9]+:[0-9]+\.[0-9]+ [0-9]+ [\.\\a-zA-Z_0-9]+:[0-9]+:",
                RegexOptions.Compiled);
            

            var logTextWriter = new LogTextWriter(i =>
            {
                ConsoleTools.LogCountInc();

                i = replace.Replace(i, "...");

                var isPing = i.Contains("ping", StringComparison.OrdinalIgnoreCase);
                var isKa = i.Contains("keepalive", StringComparison.OrdinalIgnoreCase);
                if (!isPing &&
                    !isKa &&
                    !i.Contains("close", StringComparison.OrdinalIgnoreCase) &&
                    !i.Contains("open", StringComparison.OrdinalIgnoreCase) &&
                    !i.Contains("connect", StringComparison.OrdinalIgnoreCase) &&
                    !i.Contains("fail", StringComparison.OrdinalIgnoreCase) &&
                    !i.Contains("error", StringComparison.OrdinalIgnoreCase) &&
                    //!i.Contains("send", StringComparison.OrdinalIgnoreCase) &&
                    !i.StartsWith("E", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (i.Contains("error=\"OK\"", StringComparison.OrdinalIgnoreCase) ||
                    i.Contains("connectivity", StringComparison.OrdinalIgnoreCase) ||
                    i.Contains("timer", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var fColor = i.Contains("Start keepalive ping", StringComparison.OrdinalIgnoreCase) ? ConsoleColor.Green :
                    i.Contains("Finish keepalive ping", StringComparison.OrdinalIgnoreCase) ? ConsoleColor.Blue :
                    isPing || isKa ? ConsoleColor.DarkCyan :
                    i[0] == 'D' ? ConsoleColor.DarkGray :
                    i[0] == 'E' ? ConsoleColor.DarkRed :
                    (ConsoleColor?)null;

                ConsoleTools.Log(i, fColor);
            });
            GrpcEnvironment.SetLogger(new TextWriterLogger(TextWriter.Synchronized(logTextWriter)));
        }

        public GrpcCoreClient(ITokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
            //var tw = new LogTextWriter(i => OnLog(i.ToString()));
            //var tws = TextWriter.Synchronized(tw);
            //var tl = new TextWriterLogger(() => tws);
            //GrpcEnvironment.SetLogger(new ConsoleLogger());
        }

        protected override async Task<ChannelBase> CreateChannel(string host, int port, bool useSsl)
        {
            ConsoleTools.Log("==> Create channel options...", ConsoleColor.Yellow);
            var channelOptions = new List<ChannelOption>
            {
                //new(ChannelOptions.MaxSendMessageLength, MaxMessageSize),
                //new("grpc.keepalive_time_ms", 1900),
                //new("grpc.keepalive_timeout_ms", 2000),
                //new("grpc.keepalive_permit_without_calls", 1),
                //new("grpc.http2.max_pings_without_data", 0)
                new ("GRPC_TLS_SKIP_ALL_SERVER_VERIFICATION", 1)
            };

            ConsoleTools.Log("==> Create channel...", ConsoleColor.Yellow);
            var credentials = ChannelCredentials.Create(new SslCredentials(), CallCredentials.FromInterceptor(async (c, m) =>
            {
                var token = await _tokenProvider.GetTokenAsync();
                m.Add("Authorization", $"Bearer {token}");
            }));
            var channel = new Channel(host, port, credentials, channelOptions);
            ConsoleTools.Log("==> Connecting...", ConsoleColor.Yellow);
            WatchState(channel);
            await channel.ConnectAsync();
            return channel;
        }

        private async void WatchState(Channel channel)
        {
            var state = channel.State;
            OnStatusChanged(state);
            while (true)
            {
                await channel.WaitForStateChangedAsync(state);
                state = channel.State;
                OnStatusChanged(state);

                if (state == ChannelState.Idle)
                {
                    Reconnect();
                }
            }

            async void Reconnect()
            {
                await Task.Delay(2000);
                await channel.ConnectAsync();
            }
        }
    }
}
