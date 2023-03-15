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
    private Task OnCommandCallback(HubConnection connection, string payload)
    {
      var method = System.Text.Json.JsonSerializer.Deserialize<CommandMethod>(payload);

      if (method.Command == "atriumassignment")
        OnAtriumAssignmentCallback(connection, payload);

      else if (method.Command == "roomassignment")
        OnRoomAssignmentCallback(connection, payload);

      else if (method.Command == "learnerunassignment")
        OnLearnerUnassignedCallback(connection, payload);

      return Task.CompletedTask;
    }

    private void OnAtriumAssignmentCallback(HubConnection connection, string payload)
    {
      _logger.Info($"{_param.Participant.UserId} thread: atriumassignment {payload}");
      var method = System.Text.Json.JsonSerializer.Deserialize<AtriumAssignmentCommand>(payload);

      _learner = method.Data;
    }

    private void OnRoomAssignmentCallback(HubConnection connection, string payload)
    {
      _logger.Info($"{_param.Participant.UserId} thread: roomassignment {payload}");
      var method = System.Text.Json.JsonSerializer.Deserialize<RoomAssignmentCommand>(payload);

      _roomAssigned = true;
    }

    private void OnLearnerUnassignedCallback(HubConnection connection, string payload)
    {
      _logger.Info($"{_param.Participant.UserId} thread: roomassignment {payload}");
      var method = System.Text.Json.JsonSerializer.Deserialize<RoomUnassignmentCommand>(payload);

      _roomAssigned = true;    }
  }
}
