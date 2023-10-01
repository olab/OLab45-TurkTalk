using System;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using OLab.Api.Common.Contracts;
using OLab.Api.Data.Interface;
using OLab.Api.TurkTalk.BusinessObjects;
using OLab.Api.TurkTalk.Commands;

namespace OLab.TurkTalk.Service.Azure.Functions;
public partial class Functions
{
  /// <summary>
  /// Moderator assigns a learner (remove from atrium)
  /// </summary>
  /// <param name="learner">Learner to assign</param>
  /// <param name="roomName">Room name</param>
  [Function("AssignAttendee")]
  [SignalROutput(HubName = "Hub")]
  public async Task AssignAttendee(
    [SignalRTrigger(
      "Hub", 
      "messages", 
      "AssignAttendee", 
      "learner", 
      "roomName")] 
    SignalRInvocationContext invocationContext, 
    HttpRequestData requestData,
    IUserContext userContext,
    Learner learner, 
    string roomName)
  {
    try
    {
      Guard.Argument(invocationContext).NotNull(nameof(invocationContext));
      Guard.Argument(requestData).NotNull(nameof(requestData));
      Guard.Argument(userContext).NotNull(nameof(userContext));
      Guard.Argument(learner).NotNull(nameof(learner));
      Guard.Argument(roomName).NotNull(nameof(roomName));

      _logger.LogInformation(
        $"{learner.GetUniqueKey()}: assignAttendeeAsync: '{learner.ToJson()}', {roomName}");

      var topic = _conference.GetCreateTopic(learner.TopicName, false);
      if (topic == null)
        return;

      // test if learner was removed by
      // by someone else
      if (!topic.AtriumContains(learner))
      {
        topic.Conference.SendMessage(
          new SystemMessageCommand(
            new MessagePayload(
              invocationContext.ConnectionId,
              $"Participant was already assigned")));
        return;
      }

      // remove from topic atrium
      topic.RemoveFromAtrium(learner);

      var room = topic.GetRoom(roomName);
      if (room != null)
      {
        var jumpNodes = await room.GetExitMapNodes(
          _dbContext,
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
        invocationContext.ConnectionId);

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
