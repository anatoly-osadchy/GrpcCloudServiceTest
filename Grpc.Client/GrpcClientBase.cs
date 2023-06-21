using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcCloudServiceTest;

namespace Grpc.Client
{
    delegate void LogHandler(string msg);
    internal interface IGrpcClient
    {
        Task Connect(string host, int port, bool useSsl);
        Task<string> HelloAsync(string name);

        event Action<ChannelState> StatusChanged;
        Task GetStatusesAsync(Action<string, int> nextStatus);
    }

    internal abstract class GrpcClientBase : IGrpcClient
    {
        private Greeter.GreeterClient? _client;
        private Greeter.GreeterClient Client => _client ?? throw new ArgumentNullException();

        public async Task Connect(string host, int port, bool useSsl)
        {
            ConsoleTools.Log("==> Connecting...", ConsoleColor.DarkMagenta);
            try
            {
                var channel = await CreateChannel(host, port, useSsl);
                ConsoleTools.Log("==> Connected", ConsoleColor.DarkMagenta);
                _client = new Greeter.GreeterClient(channel);
            }
            catch (Exception e)
            {
                ConsoleTools.Log("==> Connection FAILED:", ConsoleColor.Red);
                ConsoleTools.Log(e.ToString(), ConsoleColor.Red);
            }
        }

        public async Task<string> HelloAsync(string name)
        {
            ConsoleTools.Log("==> SayHello...", ConsoleColor.Blue);
            var res = await Client.SayHelloAsync(new HelloRequest{ Name = name });
            ConsoleTools.Log($"  - SayHello result: {res.Message}", ConsoleColor.Blue);
            return res.Message;
        }

        public async Task GetStatusesAsync(Action<string, int> nextStatus)
        {
            var res = Client.GetStatuses(new StatusArgs());
            while (await res.ResponseStream.MoveNext(CancellationToken.None))
            {
                var status = res.ResponseStream.Current;
                nextStatus(status.Msg, status.Progress);
            }
        }

        public event Action<ChannelState>? StatusChanged;

        protected abstract Task<ChannelBase> CreateChannel(string host, int port, bool useSsl);

        protected void OnStatusChanged(ChannelState state)
        {
            StatusChanged?.Invoke(state);
        }
    }
}
