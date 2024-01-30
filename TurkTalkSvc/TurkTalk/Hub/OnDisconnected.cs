using Common.Utils;
using Microsoft.AspNetCore.SignalR;
using OLab.Api.Common.Contracts;
using OLab.Api.TurkTalk.BusinessObjects;
using System;
using System.Threading.Tasks;

namespace OLab.Api.Services.TurkTalk
{
  /// <summary>
  /// 
  /// </summary>
  public partial class TurkTalkHub : Hub
  {
    /// <summary>
    /// A connection is disconnected from the Hub
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception exception)
    {
      try
      {
        _logger.LogDebug($"OnDisconnectedAsync: '{ConnectionIdUtils.Shorten(Context.ConnectionId)}'");

        var participant = new Participant(Context);

        // we don't know which user disconnected, so we have to search
        // the known topics by SignalR DbContext
        foreach (Topic topic in _conference.Topics)
          await topic.RemoveParticipantAsync(participant);
      }
      catch (Exception ex)
      {
        _logger.LogError($"OnDisconnectedAsync exception: {ex.Message}");
      }

      await base.OnDisconnectedAsync(exception);
    }
  }
}
