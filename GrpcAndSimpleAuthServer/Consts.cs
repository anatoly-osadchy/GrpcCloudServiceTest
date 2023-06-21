using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WebApplication1;

internal static class Consts
{
    public const string AuthScheme = JwtBearerDefaults.AuthenticationScheme;
    public const string AuthTokenKey = "AuthToken";
    
}
