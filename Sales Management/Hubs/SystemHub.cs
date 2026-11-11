using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SalesManagement.Web.Hubs
{
    public class SystemHub : Hub
    {
        public async Task SendUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveUpdate", message);
        }
    }
}
