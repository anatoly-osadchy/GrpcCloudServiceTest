using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Newtonsoft.Json.Linq;

namespace Grpc.Client;

internal class GrpcNetClient : GrpcClientBase
{
    private readonly ITokenProvider? _tokenProvider;

    public GrpcNetClient(ITokenProvider? tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<ChannelBase> CreateChannel(string host, int port, bool useSsl)
    {
        var handler = new SocketsHttpHandler
        {
            SslOptions = new SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (_, _, _, _) => true
            }
        };

        string? token = null;
        if (_tokenProvider != null && useSsl)
        {
            token = await _tokenProvider.GetTokenAsync();
            ConsoleTools.Log($"==> Token: {token}", System.ConsoleColor.Yellow);
            handler.CookieContainer.Add(new Cookie(".AspNetCore.Bearer", token, "/", "localhost") { Secure = true });
        }

        var credentials = CallCredentials.FromInterceptor((_, metadata) =>
        {
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
            return Task.CompletedTask;
        });

        var options = new GrpcChannelOptions
        {
            Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
            HttpHandler = handler
        };

        var channel = GrpcChannel.ForAddress($"http{(useSsl ? "s" : "")}://{host}:{port}", options);
        await channel.ConnectAsync();
        return channel;
    }
}
