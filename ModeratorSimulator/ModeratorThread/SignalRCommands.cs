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
  public class SignalRRoom
  {
    private IList<Learner> _learners;
    private WorkerThreadParameter _param;
    private global::NLog.ILogger _logger;
    private AuthenticateResponse _authInfo;

    public SignalRRoom(
      WorkerThreadParameter param,
      global::NLog.ILogger logger,
      AuthenticateResponse authInfo)
    {
      _learners = new List<Learner>();
      _param = param;
      _logger = logger;
      _authInfo = authInfo;
    }

  }

  public partial class ModeratorThread
  {
    private IList<Learner> _atriumLearners;
    private SignalRRoom _room;

    private void OnAtriumUpdateCommand(string json)
    {
      var commandMethod = JsonConvert.DeserializeObject<AtriumUpdateCommand>(json);
      _atriumLearners = commandMethod.Data;

      _logger.Info($"{_param.Moderator.UserId} thread: atrium update received: {commandMethod.Command}");
      _logger.Info($"{_param.Moderator.UserId} thread: atrium learner count: {_atriumLearners.Count}");


    }

    private void MethodCallbacks(HubConnection connection)
    {
      connection.On<object>("Command", (payload) =>
      {
        var json = payload.ToString();

        var commandMethod = JsonConvert.DeserializeObject<CommandMethod>(json);
        _logger.Info($"{_param.Moderator.UserId} thread: command received: {commandMethod.Command}");

        if (commandMethod.Command == "atriumupdate")
          OnAtriumUpdateCommand(json);

        return Task.CompletedTask;
      });

      connection.On<string, string, string>("message", (data, sessionId, from) =>
      {
        _logger.Info($"{_param.Moderator.UserId} thread: message {data} from {from}");
        return Task.CompletedTask;
      });

    }

  }
}
