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
using OLabWebAPI.Dto;
using OLabWebAPI.Model;

namespace OLab.TurkTalk.ParticipantSimulator
{
  public partial class ParticipantThread
  {
    private MapsFullDto _map;
    private MapsNodesFullRelationsDto _node;
    private OLabWebAPI.Dto.Designer.ScopedObjectsDto _mapScoped;
    private OLabWebAPI.Dto.Designer.ScopedObjectsDto _nodeScoped;

    public async Task<bool> MapPlayTaskAsync()
    {
      MapTrail mapTrail = _param.Participant.GetMapTrail(_param.Settings);

      var olabClient = new OLabHttpClient(_param, _authInfo);

      _map = await olabClient.LoadMapAsync(mapTrail.MapId);
      _mapScoped = await olabClient.LoadMapScopedObjectsAsync(mapTrail.MapId);

      // if no node trail, load root node
      if (mapTrail.NodeTrail == null)
      {
        int sleepMs = _param.Rnd.Next(0, mapTrail.GetDelayMs(_param.Settings));
        _logger.Debug($"{_param.Participant.UserId}: sleeping for {sleepMs} ms to play {mapTrail.MapId}/0");
        Thread.Sleep(sleepMs);

        _node = await olabClient.LoadMapNodeAsync(mapTrail);
        _nodeScoped = await olabClient.LoadMapScopedObjectsAsync(mapTrail.MapId);

        return true;
      }

      foreach (var nodeTrail in mapTrail.NodeTrail)
      {
        int sleepMs = nodeTrail.GetDelayMs(mapTrail);
        _logger.Debug($"{_param.Participant.UserId}: sleeping for {sleepMs} ms to play {mapTrail.MapId}/{nodeTrail.NodeId}");
        Thread.Sleep(sleepMs);

        _node = await olabClient.LoadMapNodeAsync(mapTrail, nodeTrail);
        _nodeScoped = await olabClient.LoadMapScopedObjectsAsync(mapTrail.MapId);

        await SignalRTask(nodeTrail);
      }

      _logger.Info($"{_param.Participant.UserId}: map play task completed");

      return true;
    }
  }
}