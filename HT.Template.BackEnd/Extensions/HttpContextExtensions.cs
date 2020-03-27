using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace HT.Template.BackEnd
{
    public struct HttpRequestData
    {
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
    }

    public static class HttpContextExtensions
    {
        public static HttpRequestData GetHttpRequestData(this HttpContext httpContext)
        {
            var info = new HttpRequestData
            {
                Method = httpContext.Request.Method,
                Path = httpContext.Request.Path
            };
            var headers = httpContext.Request.Headers.FirstOrDefault(p => p.Key == "Authorization" && p.Value.ToString().ToLower().StartsWith("bearer "));
            if (headers.Key != null)
            {
                var token = headers.Value.ToString().Substring("bearer ".Length).Trim();
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    IEnumerable<Claim> claims = handler.ReadJwtToken(token).Claims;
                    var sid = claims?.FirstOrDefault(p => p.Type == ClaimTypes.Sid)?.Value;
                    var name = claims?.FirstOrDefault(p => p.Type == ClaimTypes.Name)?.Value;
                    if (!string.IsNullOrWhiteSpace(sid))
                    {
                        info.UserId = new Guid(sid);
                        info.UserName = name;
                    }
                }
            }
            return info;
        }
    }
}