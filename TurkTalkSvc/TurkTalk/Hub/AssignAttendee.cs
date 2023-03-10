using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.TurkTalk.BusinessObjects;
using OLabWebAPI.TurkTalk.Commands;
using OLabWebAPI.TurkTalk.Contracts;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Services.TurkTalk
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
    /// <param name="routingIndex">Moderator component slot index</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task AssignAttendee(Learner learner, string roomName)
    {
      try
      {
        Guard.Argument(roomName).NotNull(nameof(roomName));

        _logger.LogInformation(
          $"AssignAttendeeAsync: '{learner.ToJson()}', {roomName} ({ConnectionId.Shorten(Context.ConnectionId)})");

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
          await room.AddLearnerAsync(learner);

        // add the moderator to the newly
        // assigned learner's group name
        await topic.Conference.AddConnectionToGroupAsync(
          learner.CommandChannel,
          Context.ConnectionId);

        // post a message to the learner that they've
        // been assigned to a room
        topic.Conference.SendMessage(
          new RoomAssignmentCommand(learner, room.Moderator ));

      }
      catch (Exception ex)
      {
        _logger.LogError($"AssignAttendeeAsync exception: {ex.Message}");
      }
    }
  }
}
