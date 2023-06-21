using GrpcCloudServiceTest.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc().AddJsonTranscoding();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
    options.ListenLocalhost(5001, listener => listener.UseHttps());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "gRPC API", Version = "v1" });

    var filePath = Path.Combine(System.AppContext.BaseDirectory, "GrpcCloudServiceTest.xml");
    options.IncludeXmlComments(filePath);
    options.IncludeGrpcXmlComments(filePath, includeControllerXmlComments: true);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.MapGrpcService<GreeterService>().RequireHost("*:5000");
app.MapGrpcService<GreeterService>().RequireHost("*:5001");
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
