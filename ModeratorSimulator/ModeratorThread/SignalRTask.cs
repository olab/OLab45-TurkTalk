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

namespace OLab.TurkTalk.ModeratorSimulator
{
  public partial class ModeratorThread
  {
    private bool _roomAssigned = false;
    private NodeTrail _nodeTrail;

    public async Task<bool> SignalRTask(NodeTrail nodeTrail)
    {
      if (nodeTrail.TurkTalkTrail == null)
        return true;

      var tmpToken = $"{_authInfo.AuthInfo.Token.Substring(0, 5)}***";
      _logger.Info($"{_param.Moderator.UserId} thread: TTalk Url. {_param.Settings.SignalRHubUrl}?access_token={tmpToken}");

      var connection = SetupConnection();

      _nodeTrail = nodeTrail;

      if (!await ConnectWithRetryAsync(connection))
        throw new Exception("Cannot connect to signal");

      if (!await RegisterModeratorAsync(connection))
        throw new Exception("Cannot register to room");

      // wait until attendee is assigned.
      while (!_roomAssigned)
      {
        _logger.Info($"{_param.Moderator.UserId} thread: checking for room assignment...");
        Thread.Sleep(10000);
      }

      _logger.Info($"{_param.Moderator.UserId} thread: room assigned");

      //if (!await SendMessagesAsync(connection, _param.Moderator, nodeTrail))
      //  throw new Exception("Failure sending messages");

      return true;
    }

    private HubConnection SetupConnection()
    {
      HubConnection connection = null;

      var url = $"{_param.Settings.SignalRHubUrl}?access_token={_authInfo.AuthInfo.Token}";
      connection = new HubConnectionBuilder()
        .WithUrl(url)
        .Build();

      _logger.Info($"{_param.Moderator.UserId} thread: created TTalk connection.");

      EventCallbacks(connection);
      MethodCallbacks(connection);

      return connection;
    }

    private async Task<bool> ConnectWithRetryAsync(HubConnection connection)
    {

      // Keep trying to until we can start or the token is canceled.
      CancellationToken token = _param.Settings.GetToken();
      while (true)
      {
        try
        {
          _logger.Info($"{_param.Moderator.UserId} thread: connecting to SignalR.");

          await connection.StartAsync(token);
          Debug.Assert(connection.State == HubConnectionState.Connected);

          _logger.Info($"{_param.Moderator.UserId} thread: connected to SignalR.  connectionId: {connection.ConnectionId}");

          return true;
        }
        catch when (token.IsCancellationRequested)
        {
          return false;
        }
        catch
        {
          _logger.Info($"{_param.Moderator.UserId} thread: failed to connect, trying again in 5000 ms.");

          Debug.Assert(connection.State == HubConnectionState.Disconnected);
          await Task.Delay(5000, token);
        }
      }
    }

    private async Task<bool> RegisterModeratorAsync(HubConnection connection)
    {
      var roomName = $"{_map.Name}|{_nodeTrail.TurkTalkTrail.RoomName}";

      await connection.InvokeAsync(
        "registerAttendee", 
        _map.Id.Value, 
        _node.Id.Value, 
        roomName,
        true);

      _logger.Info($"{_param.Moderator.UserId} thread: registered moderator for room '{roomName}'.");

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
      if (nodeTrail.TurkTalkTrail == null)
        return true;

      for (int i = 0; i < nodeTrail.TurkTalkTrail.MessageCount; i++)
      {
        var message = $"#{i+1}: {participant.UserId} {nodeTrail.TurkTalkTrail.RoomName} {RandomText()}";

        int sleepMs = nodeTrail.TurkTalkTrail.GetDelayMs(_param.Settings);
        Thread.Sleep(sleepMs);

        _logger.Info($"{_param.Moderator.UserId} thread: sending message #{i+1}/{nodeTrail.TurkTalkTrail.MessageCount} '{message}'");

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