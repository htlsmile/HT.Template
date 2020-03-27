using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HT.Template.BackEnd.Hubs
{
    public interface IApplicationHubClient
    {
        Task ReceiveMessage(string user, string message);
        Task ReceiveMessage(string message);
    }

    public class ApplicationHub : Hub<IApplicationHubClient>
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message);
        }

        public Task SendMessageToCaller(string message)
        {
            return Clients.Caller.ReceiveMessage(message);
        }
    }
}
