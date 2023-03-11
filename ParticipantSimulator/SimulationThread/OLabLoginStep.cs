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
    public async Task<AuthenticateResponse?> OLabLoginStepAsync(WorkerThreadParameter param)
    {
      if (param.Rnd == null)
        throw new ArgumentException("Missing random generator");

      int sleepMs = param.Rnd.Next(0, param.Participant.GetDelayMs(param.Settings));

      _logger.Debug($"{param.Participant.UserId} thread: sleeping for {sleepMs} ms");

      // pause for a random time up to a max time 
      Thread.Sleep(sleepMs);

      _logger.Info($"{param.Participant.UserId} thread: logging in");

      var olabClient = new OLabHttpClient(param, null);
      var loginResult = await olabClient.LoginAsync(new LoginRequest
      {
        Username = param.Participant.UserId,
        Password = param.Participant.Password
      });

      _logger.Info($"{param.Participant.UserId} thread: logged into OLab");

      return loginResult;
    }
  }
}