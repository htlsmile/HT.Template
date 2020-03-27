using System;
using HT.Template.BackEnd.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HT.Template.BackEnd.Controllers
{

    public class HomeController : AppControllerBase
    {
        public HomeController(AppDbContext context, IHubContext<ApplicationHub, IApplicationHubClient> hubContext) : base(context, hubContext)
        {
        }

        /// <summary>
        /// 获取当前时间
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Json(DateTime.Now);
        }
    }
}
