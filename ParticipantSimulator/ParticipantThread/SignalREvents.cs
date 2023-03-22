using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog;
using OLabWebAPI.Common.Contracts;
using OLabWebAPI.Model;
using OLabWebAPI.TurkTalk.BusinessObjects;
using OLabWebAPI.TurkTalk.Commands;
using OLabWebAPI.TurkTalk.Methods;

namespace OLab.TurkTalk.ParticipantSimulator
{
  public partial class ParticipantThread
  {
    private void EventCallbacks(HubConnection connection)
    {
      connection.Closed += error =>
      {
        _logger.Info($"{_param.Participant.UserId}: Connection closed");

        Debug.Assert(connection.State == HubConnectionState.Disconnected);

        // Notify users the connection has been closed or manually try to restart the connection.
        return Task.CompletedTask;
      };

      connection.Reconnecting += error =>
      {
        _logger.Info($"{_param.Participant.UserId}: Reconnecting");

        Debug.Assert(connection.State == HubConnectionState.Reconnecting);

        // Notify users the connection was lost and the client is reconnecting.
        // Start queuing or dropping messages.
        return Task.CompletedTask;
      };

      connection.Reconnected += connectionId =>
      {
        _logger.Info($"{_param.Participant.UserId}: Reconnected");

        Debug.Assert(connection.State == HubConnectionState.Connected);

        // Notify users the connection was reestablished.
        // Start dequeuing messages queued while reconnecting if any.
        return Task.CompletedTask;
      };

    }
  }
}
