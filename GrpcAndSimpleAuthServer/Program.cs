using System.Security.Claims;
using Google.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using WebApplication1;
using WebApplication1.Controllers;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddGrpc().AddJsonTranscoding();

// ADD Custom services
builder.Services.AddSingleton<IAuthService, AuthService>();

// ADD Authentication / Authorization
builder.Services.AddAuthentication(i =>
    {
        i.DefaultSignInScheme = Consts.AuthScheme;
        i.DefaultSignOutScheme = Consts.AuthScheme;
    }).AddCookie(Consts.AuthScheme);

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = Consts.AuthTokenKey;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Consts.AuthScheme, policy =>
    {
        policy.AddAuthenticationSchemes(Consts.AuthScheme);
        policy.RequireClaim(ClaimTypes.Name);
    });
});

// ADD API Controllers
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// ADD Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "gRPC API", Version = "v1" });

    var filePath = Path.Combine(System.AppContext.BaseDirectory, "WebApplication1.xml");
    c.IncludeXmlComments(filePath);
    c.IncludeGrpcXmlComments(filePath, includeControllerXmlComments: true);
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5000);
    options.ListenLocalhost(5001, listener => listener.UseHttps());
    options.ListenLocalhost(5002);
    options.ListenLocalhost(5003, listener => listener.UseHttps());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireHost("*:5003");
app.MapGrpcService<GreeterService>().RequireHost("*:5001");

app.Run();
