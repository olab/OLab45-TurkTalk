using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OLabWebAPI.Common.Contracts;
using OLabWebAPI.TurkTalk.BusinessObjects;
using OLabWebAPI.TurkTalk.Commands;
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
      Learner learner = null;
      Room room = null;

      try
      {
        Guard.Argument(payload).NotNull(nameof(payload));
        Guard.Argument(payload.RoomName).NotNull("RoomName");

        learner = new Learner(payload, Context);

        _logger.LogDebug($"{learner.GetUniqueKey()}: registerAttendee: {payload.ToJson()}");
        _logger.LogDebug($"{learner.GetUniqueKey()}: channel: {learner.CommandChannel} IP Address: {this.Context.GetHttpContext().Connection.RemoteIpAddress} node: {learner.ReferringNodeName}");

        // get or create a conference topic
        Topic topic = _conference.GetCreateTopic(learner.TopicName);
        room = topic.GetParticipantRoom(learner);

        // if no existing room contains learner, add learner to 
        // topic atrium
        if (room == null)
        {
          if (!(await topic.AddToAtriumAsync(learner)))
            _conference.SendMessage(
                Context.ConnectionId,
                new ServerErrorCommand(Context.ConnectionId, $"User is already logged in. Unable to connect."));
        }

        // user already 'known' to an existing room
        else
        {
          // if room has no moderator (i.e. moderator may have
          // disconnected) add the attendee to the topic atrium
          if (room.Moderator == null)
          {
            _logger.LogDebug($"{learner.GetUniqueKey()}: registerAttendee: room '{payload.RoomName}' has no moderator.  Assigning to atrium.");
            await topic.AddToAtriumAsync(learner);
          }

          // user already 'known' to a room AND room is moderated, so
          // signal room assignment to re-attach the learner to the room
          else
          {
            _logger.LogInformation($"{learner.GetUniqueKey()}: registerAttendee: assigning to existing room '{payload.RoomName}'");
            await AssignAttendee(learner, room.Name);
          }
        }

      }
      catch (Exception ex)
      {
        _logger.LogError($"{learner.GetUniqueKey()}: registerAttendee exception: {ex.Message}");
        _conference.SendMessage(
            Context.ConnectionId,
            new ServerErrorCommand(Context.ConnectionId, ex.Message));
      }
    }
  }
}
