using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using NLog;
using OLabWebAPI.Model;

namespace OLab.TurkTalk.ParticipantSimulator.SimulationThread
{
  public partial class SimulatorWorker
  {
    private void ProcessParticipantProcAsync(object? paramObj)
    {
      if (paramObj == null)
        throw new ArgumentException("Missing thread parameters");

      var param = (WorkerThreadParameter)paramObj;
      if (param == null)
        throw new ArgumentException("Unable to process thread parameters");

      if (param.Rnd == null)
        throw new ArgumentException("Missing Rnd parameter");

      if (param.Settings == null)
        throw new ArgumentException("Missing Settings parameter");

      if (param.CountdownEvent == null)
        throw new ArgumentException("Missing CountdownEvent parameter");
      
      try
      {
        var loginTask = OLabLoginStepAsync(param);
        loginTask.Wait();

        var authInfo = loginTask.Result;
        if (authInfo == null)
        {
          _logger.Error($"{param.Participant.UserId} thread: unable to login");
          return;
        }

        var mapPlayTask = MapPlayTaskAsync(param, authInfo);
        mapPlayTask.Wait();

      }
      catch (Exception ex)
      {
        _logger.Error($"{param.Participant.UserId} thread: exception '{ex.Message}'");
      }
      finally
      {
        // decrement the countdown event timer because 
        // the thread work has completed.
        param.CountdownEvent.Signal();
      }

    }

  }
}