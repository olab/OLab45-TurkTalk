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
    private bool _roomAssigned = false;

    private NodeTrail _nodeTrail;
    public JumpNodePayload JumpNodePayload = null;

    public async Task<bool> SignalRTask(NodeTrail nodeTrail)
    {
      var tmpToken = $"{_authInfo.AuthInfo.Token.Substring(0, 5)}***";
      _logger.Info($"{_param.Participant.UserId}: TTalk Url. {_param.Settings.SignalRHubUrl}?access_token={tmpToken}");

      _logger.Info($"{_param.Participant.UserId}: TTalk question. room: {nodeTrail.TurkTalkTrail.RoomName}");

      var connection = SetupConnection();

      _nodeTrail = nodeTrail;

      if (!await ConnectWithRetryAsync(connection))
        throw new Exception("Cannot connect to signal");

      if (!await RegisterAttendeeAsync(connection))
        throw new Exception("Cannot register to room");

      // wait until attendee is assigned.
      while (!_roomAssigned)
      {
        _logger.Info($"{_param.Participant.UserId}: checking for room assignment '{nodeTrail.TurkTalkTrail.RoomName}'");        
        Thread.Sleep(PauseMs.rnd.Next(7000, 15000));
      }

      _logger.Info($"{_param.Participant.UserId}: room assigned '{nodeTrail.TurkTalkTrail.RoomName}'");

      if (!await SendMessagesAsync(connection, _param.Participant, nodeTrail))
        throw new Exception("Failure sending messages");

      _logger.Info($"{_param.Participant.UserId}: signalR task completed");

      await connection.StopAsync();

      _roomAssigned = false;

      return true;
    }

    private HubConnection SetupConnection()
    {
      HubConnection connection = null;

      var url = $"{_param.Settings.SignalRHubUrl}?access_token={_authInfo.AuthInfo.Token}";
      connection = new HubConnectionBuilder()
        .WithAutomaticReconnect()
        .WithUrl(url)
        .Build();

      _logger.Info($"{_param.Participant.UserId}: created TTalk connection.");

      EventCallbacks(connection);
      MethodCallbacks(connection);

      return connection;
    }

    private async Task InvokeWithRetryAsync(HubConnection connection, string method, object? payload)
    {
      int retries = _param.Settings.ApiRetryCount;

      for (int i = 0; i < retries; i++)
      {
        try
        {
          await connection.InvokeAsync(method, payload);
          _logger.Info($"{_param.Participant.UserId}: invoked method {method} successfully.");
          return;
        }
        catch (Exception ex)
        {
          _logger.Warn($"{_param.Participant.UserId}: invoked {method} exception. {ex.Message}. try {i} of {retries}");
        }

        Thread.Sleep(5000);
      }

      _logger.Error($"{_param.Participant.UserId}: method {method} invoke failed.");

    }

    private async Task<bool> ConnectWithRetryAsync(HubConnection connection)
    {

      // Keep trying to until we can start or the token is canceled.
      CancellationToken token = _param.Settings.GetToken();
      while (true)
      {
        try
        {
          _logger.Info($"{_param.Participant.UserId}: connecting to SignalR.");

          await connection.StartAsync(token);
          Debug.Assert(connection.State == HubConnectionState.Connected);

          _connectionId = connection.ConnectionId;

          _logger.Info($"{_param.Participant.UserId}: connected to SignalR.  connectionId: {connection.ConnectionId}");

          return true;
        }
        catch when (token.IsCancellationRequested)
        {
          return false;
        }
        catch
        {
          _logger.Info($"{_param.Participant.UserId}: failed to connect, trying again in 5000 ms.");

          Debug.Assert(connection.State == HubConnectionState.Disconnected);
          await Task.Delay(5000, token);
        }
      }
    }

    private async Task<bool> ReregisterAttendeeAsync(HubConnection connection)
    {
      var payload = new RegisterAttendeePayload
      {
        ContextId = _node.ContextId,
        MapId = _map.Id.Value,
        NodeId = _node.Id.Value,
        RoomName = $"{_map.Name}|{_nodeTrail.TurkTalkTrail.RoomName}",
        ReferringNode = _node.Title,
        ConnectionId = _connectionId
      };

      await InvokeWithRetryAsync(connection, "reregisterAttendee", payload);

      _logger.Info($"{_param.Participant.UserId}: invoked 'reregisterAttendee' for room '{payload.RoomName}'.");

      return true;
    }

    private async Task<bool> RegisterAttendeeAsync(HubConnection connection)
    {
      var payload = new RegisterAttendeePayload
      {
        ContextId = _node.ContextId,
        MapId = _map.Id.Value,
        NodeId = _node.Id.Value,
        RoomName = $"{_map.Name}|{_nodeTrail.TurkTalkTrail.RoomName}",
        ReferringNode = _node.Title
      };

      await InvokeWithRetryAsync(connection, "registerAttendee", payload);

      _logger.Info($"{_param.Participant.UserId}: invoked 'registerAttendee' for room '{payload.RoomName}'.");

      return true;
    }

    private string lorenIpsum = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
    private static Random rnd = new Random();

    private string RandomText()
    {
      var parts = lorenIpsum.Split(' ');
      int start = rnd.Next(0, parts.Length - 10);
      int end = rnd.Next(0, 10);

      var subParts = parts.Skip(start).Take(end).ToArray();
      return string.Join(" ", subParts);
    }

    private async Task<bool> SendMessagesAsync(
      HubConnection connection,
      Participant participant,
      NodeTrail nodeTrail)
    {
      for (int i = 0; i < nodeTrail.TurkTalkTrail.MessageCount; i++)
      {
        var message = $"{i + 1}/{nodeTrail.TurkTalkTrail.MessageCount}: {participant.UserId} {nodeTrail.TurkTalkTrail.RoomName} {RandomText()}";

        int sleepMs = nodeTrail.TurkTalkTrail.GetDelayMs(nodeTrail);
        Thread.Sleep(sleepMs);

        _logger.Info($"{_param.Participant.UserId}: sending message #{i + 1}/{nodeTrail.TurkTalkTrail.MessageCount} '{message}'");

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
            QuestionId = nodeTrail.TurkTalkTrail.QuestionId,
            ContextId = _sessionId
          }
        };

        await InvokeWithRetryAsync(connection, "Message", payload);


        // test if a jump node command was received
        if (JumpNodePayload != null)
        {
          _logger.Info($"{_param.Participant.UserId}: jump node received.  Messages interrupted.");
          return true;
        }

      }

      _logger.Info($"{_param.Participant.UserId}: all messages completed");

      return true;
    }

  }
}