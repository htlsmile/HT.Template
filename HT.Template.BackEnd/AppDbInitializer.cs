using HT.Template.BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Linq;
using System.Security.Claims;

namespace HT.Template.BackEnd
{
    public class AppDbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            var appDbContext = scope.ServiceProvider.GetService<AppDbContext>();

            if (!appDbContext.Database.EnsureCreated())
            {
                return;
            }

            if (!appDbContext.Set<WebAPI>().Any())
            {
                appDbContext.AddRangeAsync(PermissionHandler.GetWebAPIs()).Wait();
                appDbContext.SaveChangesAsync().Wait();
                Log.Debug($"创建WebAPI数据，共{appDbContext.Set<WebAPI>().Count()}条");
            }

            var webAPIClaims = appDbContext.Set<WebAPI>().Select(s => new Claim(ClaimTypes.AuthenticationMethod, s.Id.ToString())).ToList();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var admin = userManager.FindByNameAsync("admin").Result;
            if (admin == null)
            {
                admin = new User
                {
                    UserName = "admin",
                    Email = "admin@email.com",
                    EmailConfirmed = true,
                    PhoneNumber = "13888888888",
                    PhoneNumberConfirmed = true
                };
                var result = userManager.CreateAsync(admin, "qwe123").Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                result = userManager.AddClaimsAsync(admin, webAPIClaims.Concat(
                    new Claim[]{
                        new Claim(ClaimTypes.Name, admin.UserName),
                        new Claim(ClaimTypes.Email, admin.Email),
                        new Claim(ClaimTypes.MobilePhone,admin.PhoneNumber)
                    })).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                Log.Debug("创建默认用户，用户名admin，密码qwe123");
            }

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var Administrator = roleManager.FindByNameAsync("Administrator").Result;
            if (Administrator == null)
            {
                Administrator = new Role
                {
                    Name = "Administrator",
                };
                var result = roleManager.CreateAsync(Administrator).Result;
                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }
                foreach (var claim in webAPIClaims)
                {
                    result = roleManager.AddClaimAsync(Administrator, claim).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                }
                Log.Debug("创建默认角色，角色名Administrator");
            }

            if (!appDbContext.Set<UserRole>().Any())
            {
                appDbContext.AddAsync(new UserRole { UserId = admin.Id, RoleId = Administrator.Id }).AsTask().Wait();
                appDbContext.SaveChangesAsync().Wait();
                Log.Debug("创建默认用户角色关联，用户名admin，角色名Administrator");
            }

        }
    }
}
