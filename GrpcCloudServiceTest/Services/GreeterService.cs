using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace GrpcCloudServiceTest.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        //[Authorize]
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        //[Authorize]
        public override async Task GetStatuses(StatusArgs request, IServerStreamWriter<Status> responseStream, ServerCallContext context)
        {
            _logger.LogInformation("==> Begin GetStatuses");
            int progress = 0;
            await Task.Delay(3000);
            while (!context.CancellationToken.IsCancellationRequested)
            {
                progress += 20;
                var status = new Status { Msg = "", Progress = progress };
                _logger.LogInformation($"==> Write status: ['{status.Msg}' / {status.Progress} %]");
                await responseStream.WriteAsync(status);
                if (progress >= 100)
                {
                    break;
                }

                await Task.Delay(60000);
            }
            _logger.LogInformation("==> Finish GetStatuses");
        }
    }
}