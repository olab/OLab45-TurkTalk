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
    public async Task<bool> MapPlayTaskAsync(WorkerThreadParameter param, AuthenticateResponse authInfo)
    {
      if (param.Participant == null)
        throw new Exception($"No participant set");

      MapTrail mapTrail = param.Participant.GetMapTrail(param.Settings);

      if (param.Rnd == null)
        throw new Exception($"No random generator set");

      var olabClient = new OLabHttpClient(param, authInfo);

      await olabClient.LoadMapAsync(mapTrail.MapId);

      // if no node trail, just load root node
      if (mapTrail.NodeTrail == null)
      {
        int sleepMs = param.Rnd.Next(0, mapTrail.GetDelayMs(param.Settings));
        _logger.Debug($"{param.Participant.UserId} thread: sleeping for {sleepMs} ms to play {mapTrail.MapId}/0");
        Thread.Sleep(sleepMs);

        await olabClient.PlayMapNodeAsync(mapTrail.MapId, 0);
        return true;
      }

      foreach (var nodeTrail in mapTrail.NodeTrail)
      {
        int sleepMs = nodeTrail.GetDelayMs(mapTrail);
        _logger.Debug($"{param.Participant.UserId} thread: sleeping for {sleepMs} ms to play {mapTrail.MapId}/{nodeTrail.NodeId}");
        Thread.Sleep(sleepMs);

        await olabClient.PlayMapNodeAsync(mapTrail.MapId, nodeTrail.NodeId);

        if (nodeTrail.TurkTalkTrail != null)
          await PlaySignalRTaskAsync(param, authInfo);
      }

      return true;
    }
  }
}