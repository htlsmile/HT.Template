using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HT.Template.BackEnd
{
    public static class JwtExtensions
    {
        public static bool IsEnabled { get; private set; }
        public static string Issuer { get; private set; }
        public static string Audience { get; private set; }
        public static byte[] SecurityKey { get; private set; }
        public static void AddAuthenticationJwt(this IServiceCollection services, IConfiguration configuration)
        {
            IsEnabled = bool.TryParse(configuration["Authentication:JwtBearer:IsEnabled"], out var b) ? b : false;
            Issuer = configuration["Authentication:JwtBearer:Issuer"];
            Audience = configuration["Authentication:JwtBearer:Audience"];
            SecurityKey = Encoding.UTF8.GetBytes(configuration["Authentication:JwtBearer:SecurityKey"]);
            if (IsEnabled)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(SecurityKey),
                        ValidateIssuer = true,
                        ValidIssuer = Issuer,
                        ValidateAudience = true,
                        ValidAudience = Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
            }
        }

    }
}