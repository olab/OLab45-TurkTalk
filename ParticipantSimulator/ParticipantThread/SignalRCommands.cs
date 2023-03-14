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
    private Task OnCommandCallback(HubConnection connection, CommandMethod method)
    {
      if (method.MethodName == "atriumassignment")
        OnAtriumAssignmentCallback(connection, method);

      else if (method.MethodName == "roomassignment")
        OnRoomAssignmentCallback(connection, method);

      else if (method.MethodName == "learnerunassignment")
        OnLearnerUnassignedCallback(connection, method);

      return Task.CompletedTask;
    }

    private void OnAtriumAssignmentCallback(HubConnection connection, CommandMethod method)
    {
      var payload = (method as AtriumAssignmentCommand).Data;

      var options = new JsonSerializerOptions { WriteIndented = false };
      string jsonString = System.Text.Json.JsonSerializer.Serialize(payload, options);

      _logger.Info($"{_param.Participant.UserId} thread: atriumassignment {jsonString}");

      _learner = payload;
    }

    private void OnRoomAssignmentCallback(HubConnection connection, CommandMethod method)
    {
      var payload = ( method as RoomAssignmentCommand).Data;

      var options = new JsonSerializerOptions { WriteIndented = false };
      string jsonString = System.Text.Json.JsonSerializer.Serialize(payload, options);
      _logger.Info($"{_param.Participant.UserId} thread: roomassignment {jsonString}");

      _roomAssigned = true;
    }

    private void OnLearnerUnassignedCallback(HubConnection connection, CommandMethod method)
    {
      var payload = ( method as RoomUnassignmentCommand).Data;

      var options = new JsonSerializerOptions { WriteIndented = false };
      string jsonString = System.Text.Json.JsonSerializer.Serialize(payload, options);
      _logger.Info($"{_param.Participant.UserId} thread: roomunassignment {jsonString}");

      _roomAssigned = true;    }
  }
}
