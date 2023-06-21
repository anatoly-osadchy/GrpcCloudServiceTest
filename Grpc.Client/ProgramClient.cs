using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grpc.Client
{
    internal class ProgramClient
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Console.WindowWidth = 250;
            //Console.WindowHeight = 75;

            //ConsoleTools.WriteCounter();

            IGrpcClient? client = null;
            ShowWaitWnd(async () =>
            {
                //var tokenProvider = (AuthTokenProvider?)null;
                var tokenProvider = new AuthTokenProvider("localhost", 5003, true);
                var t = await tokenProvider.GetTokenAsync();

                await Task.Delay(1000).ConfigureAwait(false);
                //IGrpcClient client = new GrpcCoreClient(tokenProvider);
                client = new GrpcNetClient(tokenProvider);
                client.StatusChanged += s =>
                {
                    ConsoleTools.Log($"==> Status changed to: {s}", ConsoleColor.Magenta);
                };

                await client.Connect("localhost", 5001, true);
                //await client.Connect("192.168.178.28", 5000);
            }, async () =>
            {
                if (client == null)
                {
                    return;
                }

                await SendingMsg(client!);
                //StartGetStatuses(client);
            });
        }

        private static async void StartGetStatuses(IGrpcClient client)
        {
            ConsoleTools.Log("==> Start getting statuses...", ConsoleColor.Cyan);
            await client.GetStatusesAsync((m, p) =>
            {
                ConsoleTools.Log($"==> Receive status ['{m}' / {p} %]", ConsoleColor.Cyan);
            });
            ConsoleTools.Log("==> Stop getting statuses.", ConsoleColor.Cyan);
        }

        private static int msgNum;
        private static async Task SendingMsg(IGrpcClient client)
        {
            ConsoleTools.Log("==> Sending hello msg...", ConsoleColor.Yellow);
            await client.HelloAsync($"Msg-{msgNum++}");
            ConsoleTools.Log("--> Send msg", ConsoleColor.Yellow);
        }

        static void ShowWaitWnd(Func<Task> init, Func<Task> send)
        {
            var window = new WaitWindow();
            //window.Loaded += (_, _) => handler();
            window.ButtonInit.Click += (_, _) => Do(init, "Init");
            window.ButtonSend.Click += (_, _) => Do(send, "Send");
            ConsoleTools.LogCount += i => window.Dispatcher.BeginInvoke(() => window.LogCount = i);
            ConsoleTools.OnLog += i => window.Dispatcher.BeginInvoke(() => window.Logs.Add(i));
            window.ShowDialog();

            async void Do(Func<Task> handler, string hdr)
            {
                try
                {
                    ConsoleTools.Log($"> {hdr}: starting...", ConsoleColor.DarkGray);
                    await handler();
                    ConsoleTools.Log($"> {hdr}: complete", ConsoleColor.DarkGray);
                }
                catch (Exception e)
                {
                    ConsoleTools.Log($"> {hdr}: FAIL:\r\n{e}", ConsoleColor.DarkRed);
                }
            }
        }
    }
}
