﻿using Dawn;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task<DispatchedMessages> RegisterModeratorAsync(
    RegisterParticipantRequest payload)
  {
    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));

      TtalkConferenceTopic physTopic = null;
      TtalkTopicRoom physRoom = null;

      // check if moderator is already known
      var physModerator =
        dbUnitOfWork
        .TopicParticipantRepository.GetModeratorBySessionId(payload.ContextId);

      // existing moderator
      if (physModerator != null)
      {
        physRoom =
          RoomHelper.Get(physModerator.RoomId.Value);
        physTopic = physRoom.Topic;

        // update connectionId since it's probably changed
        dbUnitOfWork
          .TopicParticipantRepository
          .UpdateConnectionId(payload.ContextId, payload.ConnectionId);
        dbUnitOfWork.Save();
      }

      // new moderator
      else
      {
        physModerator = new TtalkTopicParticipant
        {
          SessionId = payload.ContextId,
          TokenIssuer = payload.UserToken.TokenIssuer,
          UserId = payload.UserToken.UserId,
          UserName = payload.UserToken.UserName,
          NickName = payload.UserToken.NickName,
          ConnectionId = payload.ConnectionId,
          SeatNumber = 0
        };

        await dbUnitOfWork.TopicParticipantRepository.InsertAsync(physModerator);
        dbUnitOfWork.Save();

        physTopic =
          await TopicHelper.GetCreateTopicAsync(
            _conference, 
            payload.NodeId,
            payload.QuestionId);

        physRoom =
          await RoomHelper.CreateRoomAsync(
            physTopic,
            physModerator);
      }

      // create and register signalr groups
      // against the connectionId

      await TopicHelper.RegisterModeratorAsync(
        MessageQueue,
        physTopic,
        physModerator);

      RoomHelper.RegisterModerator(
        MessageQueue,
        physRoom,
        physModerator);

      return MessageQueue;

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "RegisterModeratorAsync");
      throw;
    }
    finally
    {
      dbUnitOfWork.Save();
    }
  }
}
