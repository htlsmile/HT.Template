using HT.Template.BackEnd.Hubs;
using HT.Template.BackEnd.Models;
using Microsoft.AspNetCore.SignalR;

namespace HT.Template.BackEnd.Controllers
{
    public class TestController : AppControllerBase<Test>
    {
        public TestController(AppDbContext context, IHubContext<ApplicationHub, IApplicationHubClient> hubContext) : base(context, hubContext)
        {
        }
    }
}
