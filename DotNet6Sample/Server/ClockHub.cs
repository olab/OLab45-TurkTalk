using HubServiceInterfaces;
using Microsoft.AspNetCore.SignalR;
using OLabWebAPI.TurkTalk.Commands;

namespace Server;

#region ClockHub
public class ClockHub : Hub<IClock>
{
  public async Task SendTimeToClients(AtriumAssignmentCommand payload)
  {
    var jsonPayload = payload.ToJson();
    await Clients.All.Command(jsonPayload);
  }

  public override async Task OnConnectedAsync()
  {
    Console.WriteLine($"{Context.ConnectionId} connected");
    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    Console.WriteLine($"{Context.ConnectionId} disconnected");
    await base.OnDisconnectedAsync(exception);
  }

  public void ReceiveEcho(string echoMessage)
  {
    Console.WriteLine(echoMessage);
  }
}
#endregion
