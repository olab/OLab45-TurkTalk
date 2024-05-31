using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using OLab.Api.TurkTalk.BusinessObjects;
using OLab.Api.TurkTalk.Commands;
using OLab.Api.TurkTalk.Contracts;
using System;
using System.Threading.Tasks;

namespace OLab.Api.Services.TurkTalk
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

        _logger.LogInformation($"{learner.GetUniqueKey()}: registerAttendee: {payload.ToJson()}");
        _logger.LogInformation($"{learner.GetUniqueKey()}: channel: {learner.CommandChannel} IP Address: {this.Context.GetHttpContext().Connection.RemoteIpAddress} node: {learner.ReferringNodeName}");

        // get or create a conference topic
        var topic = _conference.GetCreateTopic(learner.TopicName);

        // test if participant already in room
        room = topic.GetParticipantRoom(learner);

        // if no existing room contains learner, add learner to 
        // topic atrium
        if (room == null)
        {
          if (!(await topic.AddToAtriumAsync(learner)))
            _conference.SendMessage(
                Context.ConnectionId,
                new ServerErrorCommand(Context.ConnectionId, $"Session already exists for '{learner.NickName}'. Unable to connect."));
        }

        // user already 'known' to an existing room
        else
        {
          _conference.SendMessage(
              Context.ConnectionId,
              new ServerErrorCommand(Context.ConnectionId, $"Session already exists for '{learner.NickName}'. Unable to connect."));
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
