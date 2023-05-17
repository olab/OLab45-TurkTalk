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
    private void OnAtriumAassignedCommand(string json)
    {
      var commandMethod = JsonConvert.DeserializeObject<AtriumAssignmentCommand>(json);
      _learner = commandMethod.Data;
    }

    private void OnRoomAssignmentCommand(string json)
    {
      var commandMethod = JsonConvert.DeserializeObject<AtriumAssignmentCommand>(json);
      _roomAssigned = true;
    }

    private void OnRoomUnassignmentCommand(string json)
    {
      var commandMethod = JsonConvert.DeserializeObject<AtriumAssignmentCommand>(json);
      _roomAssigned = false;
    }

    private void OnJumpRoomCommand(string json)
    {
      JumpNodePayload = JsonConvert.DeserializeObject<JumpNodePayload>(json);      
    }    

    private void MethodCallbacks(HubConnection connection)
    {
      connection.On<object>("Command", (payload) =>
      {
        var json = payload.ToString();

        var commandMethod = JsonConvert.DeserializeObject<CommandMethod>(json);
        _logger.Info($"{_param.Participant.UserId}: command received: {commandMethod.Command}");

        if (commandMethod.Command == "atriumassignment")
          OnAtriumAassignedCommand(json);

        if (commandMethod.Command == "roomassignment")
          OnRoomAssignmentCommand(json);

        if (commandMethod.Command == "learnerunassignment")
          OnRoomUnassignmentCommand(json);

        if (commandMethod.Command == "jumpnode")
          OnJumpRoomCommand(json);
        
        return Task.CompletedTask;
      });

      connection.On<string, string, string>("message", (data, sessionId, from) =>
      {
        _logger.Info($"{_param.Participant.UserId}: message {data} from {from}");
        return Task.CompletedTask;
      });

      connection.On<string, string, string>("jumpnode", (data, sessionId, from) =>
      {
        _logger.Info($"{_param.Participant.UserId}: jumpnode {data} from {from}");
        return Task.CompletedTask;
      });
    }

  }
}
