using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using OLabWebAPI.Dto;
using OLabWebAPI.Model;
using OLabWebAPI.TurkTalk.BusinessObjects;

namespace OLab.TurkTalk.ParticipantSimulator
{
  public partial class ParticipantThread
  {
    private WorkerThreadParameter _param;
    private global::NLog.ILogger _logger;
    private AuthenticateResponse _authInfo;
    private Learner _learner;
    private OLabHttpClient _olabClient;
    private string _connectionId { get; set; }

    public ParticipantThread(WorkerThreadParameter param)
    {
      _param = param;
      _logger = param.Logger;
      _olabClient = new OLabHttpClient(_param, null);
    }

    public async Task RunProc()
    {

      try
      {
        var loginTask = await OLabLoginStepAsync();

        _authInfo = loginTask;
        if (_authInfo == null)
        {
          _logger.Error($"{_param.Participant.UserId}: unable to login");
          return;
        }

        var mapPlayTask = await MapPlayTaskAsync();

      }
      catch (Exception ex)
      {
        _logger.Error($"{_param.Participant.UserId}: exception '{ex.Message}. {ex.StackTrace}'");
      }
      finally
      {
        // decrement the countdown event timer because 
        // the thread work has completed.
        _param.CountdownEvent.Signal();
      }

    }
  }
}
