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
    private TurkTalkTrail _turkTalkTrail;
    private HubConnection _connection = null;
    private string _roomName;

    public async Task<bool> SignalRTask(NodeTrail nodeTrail)
    {
      if (nodeTrail.TurkTalkTrail == null)
        return true;

      _turkTalkTrail = nodeTrail.TurkTalkTrail;

      var tmpToken = $"{_authInfo.AuthInfo.Token.Substring(0, 5)}***";
      _logger.Info($"{_param.Moderator.UserId}: TTalk Url. {_param.Settings.SignalRHubUrl}?access_token={tmpToken}");

      _connection = SetupConnection();

      _nodeTrail = nodeTrail;

      if (!await ConnectWithRetryAsync())
        throw new Exception("Cannot connect to signal");

      if (!await RegisterModeratorAsync())
        throw new Exception("Cannot register to room");

      // wait until moderator is assigned.
      while (!_roomAssigned)
      {
        _logger.Info($"{_param.Moderator.UserId}: checking for room assignment...");
        Thread.Sleep(10000);
      }

      _logger.Info($"{_param.Moderator.UserId}: room assigned");

      // wait until simulator shutdown
      while (true)
      {
        _logger.Info($"{_param.Moderator.UserId}: sleeping");
        Thread.Sleep(10000);
      }


      //if (!await SendMessagesAsync(_connection, _param.Moderator, nodeTrail))
      //  throw new Exception("Failure sending messages");

      return true;
    }

    private HubConnection SetupConnection()
    {

      var url = $"{_param.Settings.SignalRHubUrl}?access_token={_authInfo.AuthInfo.Token}";
      _connection = new HubConnectionBuilder()
        .WithUrl(url)
        .Build();

      _logger.Info($"{_param.Moderator.UserId}: created TTalk _connection.");

      EventCallbacks();
      MethodCallbacks();

      return _connection;
    }

    private async Task<bool> ConnectWithRetryAsync()
    {

      // Keep trying to until we can start or the token is canceled.
      CancellationToken token = _param.Settings.GetToken();
      while (true)
      {
        try
        {
          _logger.Info($"{_param.Moderator.UserId}: connecting to SignalR.");

          await _connection.StartAsync(token);
          Debug.Assert(_connection.State == HubConnectionState.Connected);

          _logger.Info($"{_param.Moderator.UserId}: connected to SignalR.  connectionId: {_connection.ConnectionId}");

          return true;
        }
        catch when (token.IsCancellationRequested)
        {
          return false;
        }
        catch
        {
          _logger.Info($"{_param.Moderator.UserId}: failed to connect, trying again in 5000 ms.");

          Debug.Assert(_connection.State == HubConnectionState.Disconnected);
          await Task.Delay(5000, token);
        }
      }
    }

    private async Task<bool> RegisterModeratorAsync()
    {
      _roomName = $"{_map.Name}|{_nodeTrail.TurkTalkTrail.RoomName}";

      await _connection.InvokeAsync(
        "registerModerator", 
        _map.Id.Value, 
        _node.Id.Value, 
        _roomName,
        true);

      _logger.Info($"{_param.Moderator.UserId}: registered moderator for room '{_roomName}'.");

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

        _logger.Info($"{_param.Moderator.UserId}: sending message #{i+1}/{nodeTrail.TurkTalkTrail.MessageCount} '{message}'");

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