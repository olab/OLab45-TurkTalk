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

namespace OLab.TurkTalk.ParticipantSimulator
{
  public partial class ParticipantThread
  {
    private bool _roomAssigned = false;
    private NodeTrail _nodeTrail;

    public async Task<bool> MapNodeSignalRPlayTask(NodeTrail nodeTrail)
    {
      if (nodeTrail.TurkTalkTrail == null)
        return true;

      var tmpToken = $"{_authInfo.AuthInfo.Token.Substring(0, 5)}***";
      _logger.Info($"{_param.Participant.UserId} thread: TTalk Url. {_param.Settings.SignalRHubUrl}?access_token={tmpToken}");

      var connection = SetupConnection();

      _nodeTrail = nodeTrail;

      if (!await ConnectWithRetryAsync(connection))
        throw new Exception("Cannot connect to signal");

      if (!await RegisterAttendeeAsync(connection))
        throw new Exception("Cannot register to room");

      // wait until attendee is assigned.
      while (!_roomAssigned)
        Thread.Sleep(1000);

      if (!await SendMessagesAsync(connection, nodeTrail))
        throw new Exception("Failure sending messages");

      return true;
    }

    private HubConnection SetupConnection()
    {
      HubConnection connection = null;

      var url = $"{_param.Settings.SignalRHubUrl}?access_token={_authInfo.AuthInfo.Token}";
      connection = new HubConnectionBuilder()
        .WithUrl(url)
        .Build();

      _logger.Info($"{_param.Participant.UserId} thread: created TTalk connection.");

      connection.Closed += error =>
      {
        _logger.Info($"{_param.Participant.UserId} thread: Connection closed");

        Debug.Assert(connection.State == HubConnectionState.Disconnected);

        // Notify users the connection has been closed or manually try to restart the connection.
        return Task.CompletedTask;
      };

      connection.Reconnecting += error =>
      {
        _logger.Info($"{_param.Participant.UserId} thread: Reconnecting");

        Debug.Assert(connection.State == HubConnectionState.Reconnecting);

        // Notify users the connection was lost and the client is reconnecting.
        // Start queuing or dropping messages.
        return Task.CompletedTask;
      };

      connection.Reconnected += connectionId =>
      {
        _logger.Info($"{_param.Participant.UserId} thread: Reconnected");

        Debug.Assert(connection.State == HubConnectionState.Connected);

        // Notify users the connection was reestablished.
        // Start dequeuing messages queued while reconnecting if any.
        return Task.CompletedTask;
      };

      return connection;
    }

    private async Task<bool> ConnectWithRetryAsync(HubConnection connection)
    {
      connection.On<Learner>("atriumassignment", (learner) =>
      {
        var options = new JsonSerializerOptions { WriteIndented = false };
        string jsonString = System.Text.Json.JsonSerializer.Serialize(learner, options);
        _logger.Info($"{_param.Participant.UserId} thread: atriumassignment {jsonString}");

        _learner = learner;
      });

      connection.On<RoomAssignmentPayload>("roomassignment", (payload) =>
      {
        var options = new JsonSerializerOptions { WriteIndented = false };
        string jsonString = System.Text.Json.JsonSerializer.Serialize(payload, options);
        _logger.Info($"{_param.Participant.UserId} thread: roomassignment {jsonString}");

        _roomAssigned = true;
      });

      connection.On<string, string, string>("message", (data, sessionId, from) =>
      {
        _logger.Info($"{_param.Participant.UserId} thread: message {data} from {from}");
      });

      connection.On<string, string, string>("jumpnode", (data, sessionId, from) =>
      {
        _logger.Info($"{_param.Participant.UserId} thread: jumpnode {data} from {from}");
      });

      connection.On<Participant, int>("learnerunassignment", (participant, slotIndex) =>
      {
        var options = new JsonSerializerOptions { WriteIndented = false };
        string jsonString = System.Text.Json.JsonSerializer.Serialize(participant, options);
        _logger.Info($"{_param.Participant.UserId} thread: learnerunassignment {jsonString} index {slotIndex}");
      });

      // Keep trying to until we can start or the token is canceled.
      CancellationToken token = _param.Settings.GetToken();
      while (true)
      {
        try
        {
          _logger.Info($"{_param.Participant.UserId} thread: attempting to connect to SignalR.");

          await connection.StartAsync(token);
          Debug.Assert(connection.State == HubConnectionState.Connected);

          _logger.Info($"{_param.Participant.UserId} thread: connected to SignalR.");

          return true;
        }
        catch when (token.IsCancellationRequested)
        {
          return false;
        }
        catch
        {
          _logger.Info($"{_param.Participant.UserId} thread: failed to connect, trying again in 5000 ms.");

          Debug.Assert(connection.State == HubConnectionState.Disconnected);
          await Task.Delay(5000, token);
        }
      }
    }
    private async Task<bool> RegisterAttendeeAsync(HubConnection connection)
    {
      var payload = new RegisterAttendeePayload
      {
        ContextId = _node.ContextId,
        MapId = _map.Id.Value,
        NodeId = _node.Id.Value,
        QuestionId = _nodeTrail.TurkTalkTrail.QuestionId,
        RoomName = $"{_map.Name}|{_nodeTrail.TurkTalkTrail.RoomName}"
      };

      _logger.Info($"{_param.Participant.UserId} thread: attempting to register as an attendee to room {payload.RoomName}.");

      await connection.InvokeAsync("registerAttendee", payload);

      return true;
    }

    private async Task<bool> SendMessagesAsync(
      HubConnection connection,
      NodeTrail nodeTrail)
    {
      if (nodeTrail.TurkTalkTrail == null)
        return true;

      for (int i = 0; i < nodeTrail.TurkTalkTrail.MessageCount; i++)
      {
        var message = $"Sim message {i}";

        int sleepMs = nodeTrail.TurkTalkTrail.GetDelayMs(nodeTrail);
        Thread.Sleep(sleepMs);

        _logger.Info($"{_param.Participant.UserId} thread: sending message '{message}'");

        var payload = new MessagePayload
        {
          Data = message,
          Envelope = new Envelope
          {
            To = _learner.CommandChannel,
            From = _learner
          },
          Session = new SessionInfo
          {
            MapId = _map.Id.Value,
            NodeId = _node.Id.Value,
            ContextId = _node.ContextId
          }
        };

        await connection.InvokeAsync(
          "Message",
          payload);
      }

      return true;
    }

  }
}