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

namespace OLab.TurkTalk.ParticipantSimulator
{
  public partial class ParticipantThread
  {
    public async Task<AuthenticateResponse> OLabLoginStepAsync()
    {
      int sleepMs = _param.Rnd.Next(0, _param.Participant.GetDelayMs(_param.Settings));

      _logger.Debug($"{_param.Participant.UserId}: sleeping for {sleepMs} ms");

      // pause for a random time up to a max time 
      Thread.Sleep(sleepMs);

      _logger.Info($"{_param.Participant.UserId}: logging in");

      var loginResult = await _olabClient.LoginAsync(new LoginRequest
      {
        Username = _param.Participant.UserId,
        Password = _param.Participant.Password
      });

      _logger.Info($"{_param.Participant.UserId}: logged into OLab");

      return loginResult;
    }
  }
}