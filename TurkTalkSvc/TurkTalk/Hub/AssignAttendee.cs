using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OLab.Api.TurkTalk.BusinessObjects;
using OLab.Api.TurkTalk.Commands;
using OLab.Api.Common.Contracts;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using OLab.Api.Utils;

namespace OLab.Api.Services.TurkTalk
{
  /// <summary>
  /// 
  /// </summary>
  public partial class TurkTalkHub : Hub
  {
    /// <summary>
    /// Moderator assigns a learner (remove from atrium)
    /// </summary>
    /// <param name="learner">Learner to assign</param>
    /// <param name="roomName">Room name</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task AssignAttendee(Learner learner, string roomName)
    {
      try
      {
        Guard.Argument(roomName).NotNull(nameof(roomName));

        _logger.LogInformation(
          $"{learner.GetUniqueKey()}: assignAttendeeAsync: '{learner.ToJson()}', {roomName}");

        Topic topic = _conference.GetCreateTopic(learner.TopicName, false);
        if (topic == null)
          return;

        // test if learner was removed by
        // by someone else
        if (!topic.AtriumContains(learner))
        {
          topic.Conference.SendMessage(
            new SystemMessageCommand(
              new MessagePayload(
                Context.ConnectionId,
                $"Participant was already assigned")));
          return;
        }

        // remove from topic atrium
        topic.RemoveFromAtrium(learner);

        Room room = topic.GetRoom(roomName);
        if (room != null)
        {
          var userContext = GetUserContext();
          var jumpNodes = await room.GetExitMapNodes(
            Context.GetHttpContext(), 
            userContext, 
            learner.Session.MapId, 
            learner.Session.NodeId);

          if (!(await room.AddLearnerAsync(learner, jumpNodes)))
            return;
        }

        // add the moderator to the newly
        // assigned learner's group name
        await topic.Conference.AddConnectionToGroupAsync(
          learner.CommandChannel,
          Context.ConnectionId);

        // post a message to the learner that they've
        // been assigned to a room
        topic.Conference.SendMessage(
          new RoomAssignmentCommand(learner, room.Moderator));

      }
      catch (Exception ex)
      {
        _logger.LogError($"{learner.GetUniqueKey()}: assignAttendeeAsync exception: {ex.Message}");
      }
    }
  }
}
