using HT.Template.BackEnd.Hubs;
using HT.Template.BackEnd.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace HT.Template.BackEnd
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();

            services.AddDbContext<AppDbContext>(builder => builder.Configure());

            services.AddIdentity<User, Role>(options =>
            {
                options.Password = new PasswordOptions
                {
                    RequireDigit = true,
                    RequiredLength = 6,
                    RequiredUniqueChars = 1,
                    RequireLowercase = false,
                    RequireNonAlphanumeric = false,
                    RequireUppercase = false
                };
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            }).AddEntityFrameworkStores<AppDbContext>();

            services.AddControllers().AddJsonOptions(o => o.Configure());

            services.AddCors(o => o.AddDefaultPolicy(p =>
            p.SetIsOriginAllowedToAllowWildcardSubdomains()
             .WithOrigins(Configuration.GetSection("AllowOrigins").Get<string[]>())
             .AllowAnyHeader()
             .AllowAnyMethod()));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = $"{Config.AssemblyName} API",
                    Description = $"服务启动时间：{DateTime.Now}",
                    Contact = new OpenApiContact
                    {
                        Name = "htlsmile",
                        Email = "htlsmile@outlook.com",
                        Url = new Uri("https://htlsmile.github.io/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://opensource.org/licenses/mit-license.php")
                    }
                });
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Jwt Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Config.AssemblyName}.xml"));
            });

            services.AddAuthenticationJwt(Configuration);
            services.AddAuthorization(o => o.AddPolicy(nameof(PermissionRequirement), policy => policy.Requirements.Add(new PermissionRequirement(true))));
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            app.UseExceptionHandler(options => options.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json;charset=utf-8";
                context.Response.Headers["Access-Control-Allow-Origin"] = "*";
                var ex = context.Features.Get<IExceptionHandlerFeature>();
                if (ex != null)
                {
                    var errorObj = new
                    {
                        message = ex.Error.Message,
                        stackTrace = ex.Error.StackTrace,
                        exceptionType = ex.Error.GetType().Name
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorObj)).ConfigureAwait(false);
                }
            }));

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{env.ApplicationName} API V1");
                c.RoutePrefix = string.Empty;
                c.DocExpansion(DocExpansion.List);
            });

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ApplicationHub>("/hub");
            });
        }
    }
}
