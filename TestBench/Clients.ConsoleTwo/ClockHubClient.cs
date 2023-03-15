using HubServiceInterfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OLabWebAPI.TurkTalk.Commands;
using OLabWebAPI.TurkTalk.Methods;

namespace Clients.ConsoleTwo
{
  public class ClockHubClient : IClock, IHostedService
  {
    #region ClockHubClientCtor
    private readonly ILogger<ClockHubClient> _logger;
    private HubConnection _connection;

    public ClockHubClient(ILogger<ClockHubClient> logger)
    {
      _logger = logger;

      _connection = new HubConnectionBuilder()
          .WithUrl(Strings.HubUrl)
          .Build();

      _connection.On<string>(Strings.Events.TimeSent, Command);
    }

    public async Task Command(string payload)
    {
      try
      {
        _logger.LogInformation($"{DateTime.UtcNow.ToShortTimeString()} {payload}");
        var method = JsonConvert.DeserializeObject<AtriumAssignmentCommand>(payload);

        await _connection.InvokeAsync("ReceiveEcho", payload);

      }
      catch (Exception ex)
      {
        _logger.LogInformation($"{ex.Message}");
        throw;
      }


      return;
    }
    #endregion

    #region StartAsync
    public async Task StartAsync(CancellationToken cancellationToken)
    {
      // Loop is here to wait until the server is running
      while (true)
      {
        try
        {
          await _connection.StartAsync(cancellationToken);

          break;
        }
        catch
        {
          await Task.Delay(1000, cancellationToken);
        }
      }
    }
    #endregion
    #region StopAsync
    public async Task StopAsync(CancellationToken cancellationToken)
    {
      await _connection.DisposeAsync();
    }
    #endregion
  }
}
