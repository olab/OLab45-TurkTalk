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
    private string _sessionId;

    public async Task<bool> MapPlayTaskAsync()
    {
      MapTrail mapTrail = _param.Participant.GetMapTrail(_param.Settings);

      _map = await _olabClient.LoadMapAsync(mapTrail.MapId);
      _mapScoped = await _olabClient.LoadMapScopedObjectsAsync(mapTrail.MapId);

      // if no node trail, load root node
      if (mapTrail.NodeTrail == null)
      {
        int sleepMs = _param.Rnd.Next(0, mapTrail.GetDelayMs(_param.Settings));
        //_logger.Debug($"{_param.Participant.UserId}: sleeping for {sleepMs} ms to play {mapTrail.MapId}/0");
        Thread.Sleep(sleepMs);

        _node = await _olabClient.LoadMapNodeAsync(mapTrail);
        _nodeScoped = await _olabClient.LoadMapScopedObjectsAsync(mapTrail.MapId);

        return true;
      }

      foreach (var nodeTrail in mapTrail.NodeTrail)
      {
        // test if redirecting to a jump node
        if (JumpNodePayload != null)
        {
          // if not on the jump node yet, skip current node
          if (nodeTrail.NodeId != JumpNodePayload.Data.NodeId)
          {
            _logger.Debug($"{_param.Participant.UserId}: skipping until jump node {JumpNodePayload.Data.NodeId}");
            continue;
          }
          else
            _logger.Debug($"{_param.Participant.UserId}: playing jump node {JumpNodePayload.Data.NodeId}");
        }

        int sleepMs = nodeTrail.GetDelayMs(mapTrail);
        //_logger.Debug($"{_param.Participant.UserId}: sleeping for {sleepMs} ms to play {mapTrail.MapId}/{nodeTrail.NodeId}");
        Thread.Sleep(sleepMs);

        _node = await _olabClient.LoadMapNodeAsync(mapTrail, nodeTrail);

        if (_node == null)
        {
          _logger.Error($"{_param.Participant.UserId}: could not get node {nodeTrail.NodeId}");
          continue;
        }

        if (string.IsNullOrEmpty(_sessionId))
        {
          _sessionId = _node.ContextId;
          _logger.Debug($"{_param.Participant.UserId}: sessionId: {_sessionId}");
        }

        _nodeScoped = await _olabClient.LoadMapScopedObjectsAsync(mapTrail.MapId);

        // test if there's a turk talk question in the node
        if (nodeTrail.TurkTalkTrail != null)
          await SignalRTask(nodeTrail);

        // test if jsut ran the jump node
        if (JumpNodePayload != null)
        {
          // if not on the jump node yet, skip current node
          if (nodeTrail.NodeId == JumpNodePayload.Data.NodeId)
          {
            _logger.Debug($"{_param.Participant.UserId}: resuming node trail after jump node {JumpNodePayload.Data.NodeId}");
            JumpNodePayload = null;
          }
        }
      }

      _logger.Info($"{_param.Participant.UserId}: map play task completed");

      return true;
    }
  }
}