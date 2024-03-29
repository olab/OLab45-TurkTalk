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
    public async Task ReregisterAttendee(RegisterAttendeePayload payload)
    {
      Learner learner = null;
      Room room = null;

      try
      {
        Guard.Argument(payload).NotNull(nameof(payload));
        Guard.Argument(payload.RoomName).NotNull("RoomName");

        learner = new Learner(payload, Context);

        _logger.LogDebug($"{learner.GetUniqueKey()}: reregisterAttendee: {payload.ToJson()}");
        _logger.LogDebug($"{learner.GetUniqueKey()}: channel: {learner.CommandChannel} IP Address: {this.Context.GetHttpContext().Connection.RemoteIpAddress} node: {learner.ReferringNodeName}");

        // get or create a conference topic
        Topic topic = _conference.GetCreateTopic(learner.TopicName);
        room = topic.GetParticipantRoom(learner);

        // if no existing room contains learner, add learner to 
        // topic atrium
        if (room == null)
          await topic.AddToAtriumAsync(learner);

      }
      catch (Exception ex)
      {
        _logger.LogError($"{learner.GetUniqueKey()}: registerAttendee exception: {ex.Message}");

        if ( ( room != null) && ( learner != null ) )
        {
          room.Topic.Conference.SendMessage(
            Context.ConnectionId, 
            new ServerErrorCommand(Context.ConnectionId, ex.Message));
        }
      }
    }
  }
}
