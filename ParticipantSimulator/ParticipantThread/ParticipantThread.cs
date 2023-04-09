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

    public ParticipantThread(WorkerThreadParameter param)
    {
      _param = param;
      _logger = param.Logger;
    }

    public void RunProc()
    {

      try
      {
        var loginTask = OLabLoginStepAsync();
        loginTask.Wait();

        _authInfo = loginTask.Result;
        if (_authInfo == null)
        {
          _logger.Error($"{_param.Participant.UserId}: unable to login");
          return;
        }

        var mapPlayTask = MapPlayTaskAsync();
        mapPlayTask.Wait();

      }
      catch (Exception ex)
      {
        _logger.Error($"{_param.Participant.UserId}: exception '{ex.Message}'");
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
