using PCautivoApi.Shared.Filters;
using PCautivoCore.Shared.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace PCautivoApi.Modules;

public static class AuthModule
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"] ?? string.Empty))
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (int.TryParse(userIdClaim, out int userId))
                    {
                        AuthContext authContext = new()
                        {
                            UserId = userId
                        };

                        context.HttpContext.RequestServices.GetService<IAuthContext>()?.SetAuthContext(authContext);
                    }

                    await Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Bearer", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
               policy.AddRequirements(new PermissionRequirement());
            });
        });

        return services;
    }
}
