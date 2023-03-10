using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.TurkTalk.BusinessObjects;
using OLabWebAPI.TurkTalk.Contracts;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace OLabWebAPI.Services.TurkTalk
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TurkTalkHub : Hub
  {
    /// <summary>
    /// Register attendee to room
    /// </summary>
    /// <param name="roomName">Room name</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task RegisterAttendee(RegisterAttendeePayload payload)
    {
      try
      {
        Guard.Argument(payload).NotNull(nameof(payload));
        Guard.Argument(payload.RoomName).NotNull("RoomName");

        var learner = new Learner(payload, Context);

        _logger.LogDebug($"RegisterAttendee: room: {payload.ToJson()} '{learner.CommandChannel} ({ConnectionId.Shorten(Context.ConnectionId)})");

        // get or create a conference topic
        Topic topic = _conference.GetCreateTopic(learner.TopicName);
        Room room = topic.GetParticipantRoom(learner);

        // if no existing room contains learner, add learner to 
        // topic atrium
        if (room == null)
        {
          _logger.LogDebug($"RegisterAttendee: adding to '{payload.RoomName}' atrium");
          await topic.AddToAtriumAsync(learner);
        }

        // user already 'known' to an existing room
        else
        {

          // if room has no moderator (i.e. moderator may have
          // disconnected) add the attendee to the topic atrium
          if (room.Moderator == null)
          {
            _logger.LogDebug($"RegisterAttendee: room '{payload.RoomName}' has no moderator.  Assigning to atrium.");
            await topic.AddToAtriumAsync(learner);
          }

          // user already 'known' to a room AND room is moderated, so
          // signal room assignment to re-attach the learner to the room
          else
          {
            _logger.LogInformation($"RegisterAttendee: assigning Participant to existing room '{payload.RoomName}'");
            await AssignAttendee(learner, room.Name);
          }
        }

      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterAttendee exception: {ex.Message}");
      }
    }
  }
}
