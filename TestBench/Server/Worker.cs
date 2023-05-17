using HubServiceInterfaces;
using Microsoft.AspNetCore.SignalR;
using OLabWebAPI.TurkTalk.BusinessObjects;
using OLabWebAPI.TurkTalk.Commands;

namespace Server;

#region Worker
public class Worker : BackgroundService
{
  private readonly ILogger<Worker> _logger;
  private readonly IHubContext<ClockHub, IClock> _clockHub;

  public Worker(ILogger<Worker> logger, IHubContext<ClockHub, IClock> clockHub)
  {
    _logger = logger;
    _clockHub = clockHub;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      _logger.LogInformation("Worker running at: {Time}", DateTime.Now);

      var learner = new Learner();
      learner.UserId = DateTime.Now.ToShortTimeString();

      var moderator = new Learner();
      moderator.UserId = DateTime.Now.ToShortDateString();

      var payload = new AtriumAssignmentCommand(moderator, learner);

      await _clockHub.Clients.All.Command(payload.ToJson());
      await Task.Delay(1000, stoppingToken);
    }
  }
}
#endregion
