using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HT.Template.BackEnd.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace HT.Template.BackEnd
{
    public class TaskManager : BackgroundService
    {
        public TaskManager(IHubContext<ApplicationHub, IApplicationHubClient> hubContext) => HubContext = hubContext;

        public IHubContext<ApplicationHub, IApplicationHubClient> HubContext { get; }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Task.Delay(1000).Wait();
                var message = $"{DateTime.Now.ToLongTimeString()}:来自后台的公告消息";
                //Log.Debug($"向所有客户端发送 [{message}]");
                HubContext.Clients.All.ReceiveMessage("系统", message);
            }
            Log.Information($"后台服务运行结束");
            return Task.CompletedTask;
        }
    }
}
