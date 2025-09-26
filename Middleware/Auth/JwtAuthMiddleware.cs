using ASP_32.Data.Entities;
using ASP_32.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_32.Middleware.Auth
{
    public class JwtAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            String authHeader = context.Request.Headers.Authorization.ToString();
            if( ! String.IsNullOrEmpty(authHeader))
            {
                String scheme = "Bearer ";
                if(authHeader.StartsWith(scheme))
                {
                    String? errorMessage = null;
                    String jwt = authHeader[scheme.Length..];
                    String[] parts = jwt.Split('.');
                    if(parts.Length == 3)
                    {
                        // Перевіряємо підпис
                        String tokenBody = parts[0] + '.' + parts[1];
                        String secret = configuration.GetSection("Jwt").GetSection("Secret").Value
                        ?? throw new KeyNotFoundException("Not found configuration 'Jwt.Secret'");
                        String signature = Base64UrlTextEncoder.Encode(
                            System.Security.Cryptography.HMACSHA256.HashData(
                                System.Text.Encoding.UTF8.GetBytes(secret),
                                System.Text.Encoding.UTF8.GetBytes(tokenBody)
                        ));
                        if(signature == parts[2])
                        {
                            String payload = System.Text.Encoding.UTF8.GetString(
                                Base64UrlTextEncoder.Decode(parts[1]));
                            var data = JsonSerializer.Deserialize<JsonElement>(payload)!;
                            context.User = new ClaimsPrincipal(
                                new ClaimsIdentity(
                                    [
                                        new Claim(ClaimTypes.PrimarySid,  data.GetString("sub")! ),
                                        new Claim(ClaimTypes.Name, data.GetString("name")!),
                                        new Claim(ClaimTypes.Role, data.GetString("aud")! ),
                                    ],
                                    nameof(JwtAuthMiddleware)
                                )
                            );
                        }
                        else
                        {
                            errorMessage = "Invalid JWT signature";
                        }
                    }
                    else
                    {
                        errorMessage = "Invalid JWT structure";
                    }
                    if(errorMessage != null)
                    {
                        context.Response.Headers.Append(
                            "Authentication-Control",
                            errorMessage
                        );
                    }
                }
            }            

            await _next(context);
        }
    }


    public static class JwtAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtAuth(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtAuthMiddleware>();
        }
    }
}
/* Д.З. Реалізувати авторизацію засобами JWT у власному 
 * курсовому проєкті. Прикласти посилання на репозиторії
 * проєктів (бек, фронт)
 */
