using HT.Template.BackEnd.Hubs;
using HT.Template.BackEnd.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HT.Template.BackEnd
{
    public class AppTaskManager : BackgroundService
    {
        public AppTaskManager(IHubContext<ApplicationHub, IApplicationHubClient> hubContext) => HubContext = hubContext;

        public IHubContext<ApplicationHub, IApplicationHubClient> HubContext { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var tasks = from type in Assembly.GetExecutingAssembly().GetTypes()
                            where typeof(AppTaskBase).IsAssignableFrom(type) && !type.IsAbstract
                            select Activator.CreateInstance(type) as AppTaskBase;
                stoppingToken.Register(() =>
                {
                    foreach (var task in tasks)
                    {
                        task.Stop();
                    }
                    HubContext.Clients.All.ReceiveMessage("后台服务已停止");
                });
                await Task.WhenAll(tasks.Select(s => s.Start()));
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    Log.Error(ex, ex.Message);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Log.Error(ex,ex.Message);
            }
        }
    }

}