using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;

namespace OLab.TurkTalk.ModeratorSimulator
{
  public partial class ModeratorThread
  {
    private void EventCallbacks()
    {
      _connection.Closed += error =>
      {
        _logger.Info( $"{_param.Moderator.UserId}: Connection closed" );

        Debug.Assert( _connection.State == HubConnectionState.Disconnected );

        // Notify users the _connection has been closed or manually try to restart the _connection.
        return Task.CompletedTask;
      };

      _connection.Reconnecting += error =>
      {
        _logger.Info( $"{_param.Moderator.UserId}: Reconnecting" );

        Debug.Assert( _connection.State == HubConnectionState.Reconnecting );

        // Notify users the _connection was lost and the client is reconnecting.
        // Start queuing or dropping messages.
        return Task.CompletedTask;
      };

      _connection.Reconnected += connectionId =>
      {
        _logger.Info( $"{_param.Moderator.UserId}: Reconnected" );

        Debug.Assert( _connection.State == HubConnectionState.Connected );

        // Notify users the _connection was reestablished.
        // Start dequeuing messages queued while reconnecting if any.
        return Task.CompletedTask;
      };

    }
  }
}
