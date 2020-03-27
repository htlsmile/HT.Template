﻿using HT.Template.BackEnd.Hubs;
using HT.Template.BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Threading.Tasks;

namespace HT.Template.BackEnd.Controllers
{
    public class RoleController : AppControllerBase
    {
        private readonly RoleManager<Role> roleManager;

        public RoleController(AppDbContext context,
                              IHubContext<ApplicationHub, IApplicationHubClient> hubContext,
                              RoleManager<Role> roleManager) : base(context, hubContext)
        {
            this.roleManager = roleManager;
        }

        /// <summary>
        /// 刷新API信息
        /// </summary>
        /// <returns></returns>
        [HttpPut(nameof(RefreshWebAPI))]
        public async Task<IActionResult> RefreshWebAPI()
        {
            var webAPIs = from webAPI in PermissionHandler.GetWebAPIs()
                          where !Repository.Exist<WebAPI>(p => p.Equals(webAPI))
                          select webAPI;
            var count = webAPIs.Count();
            if (count > 0)
            {
                var result = await Repository.InsertRangeAsync(webAPIs);
                return Ok(new APIResult(result, $"已新增{count}条WebAPI"));
            }
            else
            {
                return Ok(new APIResult(true, "没有发现新的WebAPI"));
            }

        }
    }
}
