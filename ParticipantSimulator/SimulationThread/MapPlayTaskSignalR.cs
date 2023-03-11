using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using NLog;
using OLabWebAPI.Model;

namespace OLab.TurkTalk.ParticipantSimulator.SimulationThread
{
  public partial class SimulatorWorker
  {
    public async Task<bool> PlaySignalRTaskAsync(WorkerThreadParameter param, AuthenticateResponse authInfo)
    {
      var tmpToken = $"{authInfo.AuthInfo.Token.Substring(0, 5)}***";
      _logger.Info($"{param.Participant.UserId} thread: TTalk Url. {param.Settings.SignalRHubUrl}?access_token={tmpToken}");

      var url = $"{param.Settings.SignalRHubUrl}?access_token={authInfo.AuthInfo.Token}";
      HubConnection connection = new HubConnectionBuilder()
        .WithUrl(url)
        .Build();

      _logger.Info($"{param.Participant.UserId} thread: created TTalk connection.");

      connection.Closed += error =>
      {
        _logger.Info($"{param.Participant.UserId} thread: Connection closed");

        Debug.Assert(connection.State == HubConnectionState.Disconnected);

        // Notify users the connection has been closed or manually try to restart the connection.
        return Task.CompletedTask;
      };

      connection.Reconnecting += error =>
      {
        _logger.Info($"{param.Participant.UserId} thread: Reconnecting");

        Debug.Assert(connection.State == HubConnectionState.Reconnecting);

        // Notify users the connection was lost and the client is reconnecting.
        // Start queuing or dropping messages.
        return Task.CompletedTask;
      };

      connection.Reconnected += connectionId =>
      {
        _logger.Info($"{param.Participant.UserId} thread: Reconnected");

        Debug.Assert(connection.State == HubConnectionState.Connected);

        // Notify users the connection was reestablished.
        // Start dequeuing messages queued while reconnecting if any.
        return Task.CompletedTask;
      };

      if (!await ConnectWithRetryAsync(connection, param))
        throw new Exception("Cannot connect to signal");

      return true;
    }

    public async Task<bool> ConnectWithRetryAsync(HubConnection connection, WorkerThreadParameter param)
    {
      // Keep trying to until we can start or the token is canceled.
      CancellationToken token = param.Settings.GetToken();
      while (true)
      {
        try
        {
          _logger.Info($"{param.Participant.UserId} thread: attempting to connect to SignalR.");

          await connection.StartAsync(token);
          Debug.Assert(connection.State == HubConnectionState.Connected);

          _logger.Info($"{param.Participant.UserId} thread: connected to SignalR.");

          return true;
        }
        catch when (token.IsCancellationRequested)
        {
          return false;
        }
        catch
        {
          _logger.Info($"{param.Participant.UserId} thread: failed to connect, trying again in 5000 ms.");

          Debug.Assert(connection.State == HubConnectionState.Disconnected);
          await Task.Delay(5000, token);
        }
      }
    }
  }
}