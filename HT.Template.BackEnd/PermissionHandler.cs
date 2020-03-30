using HT.Template.BackEnd.Controllers;
using HT.Template.BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HT.Template.BackEnd
{
    /// <summary>
    /// 权限处理
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        public PermissionHandler(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        public IHttpContextAccessor HttpContextAccessor { get; }

        /// <summary>
        /// 权限处理
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var data = HttpContextAccessor.HttpContext.GetHttpRequestData();
            if (data.UserId.HasValue)
            {
                if (data.UserName == "admin")
                {
                    context.Succeed(requirement);
                }
                else
                {
                    using var dbContext = new AppDbContext();
                    var webAPI = dbContext.Set<WebAPI>().FirstOrDefault(p => p.Method == data.Method && p.Path == data.Path);
                    var userClaims = from uc in dbContext.UserClaims.Where(p => p.UserId == data.UserId)
                                     where uc.ClaimType == ClaimTypes.AuthenticationMethod && uc.ClaimValue == webAPI.Id.ToString()
                                     select uc;
                    var roleClaims = from ur in dbContext.UserRoles.Where(p => p.UserId == data.UserId)
                                     from rc in dbContext.RoleClaims.Where(p => p.RoleId == ur.RoleId)
                                     where rc.ClaimType == ClaimTypes.AuthenticationMethod && rc.ClaimValue == webAPI.Id.ToString()
                                     select rc;
                    if (userClaims.ToList().Count > 0 || roleClaims.ToList().Count > 0)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public static IEnumerable<WebAPI> GetWebAPIs() =>
            from type in Assembly.GetExecutingAssembly().GetTypes()
            where typeof(AppControllerBase).IsAssignableFrom(type) && !type.IsAbstract
            from method in type.GetMethods()
            where method.IsPublic && method.CustomAttributes.Any(p => typeof(HttpMethodAttribute).IsAssignableFrom(p.AttributeType))
            let attribute = method.GetCustomAttribute<HttpMethodAttribute>()
            let routing = $"/{type.Name.Replace(nameof(Microsoft.AspNetCore.Mvc.Controller), "")}/{attribute.Template}".TrimEnd('/')
            let httpMethod = Enumerable.FirstOrDefault<string>(attribute.HttpMethods) ?? "GET"
            let name = $"{type.FullName}.{method.Name}"
            select new WebAPI
            {
                Name = name,
                Description = XMLDoc.Current.GetComment((string)name),
                Path = routing,
                Method = httpMethod
            };

    }
}