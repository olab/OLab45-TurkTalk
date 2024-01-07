using Dawn;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using OLab.Api.Common.Contracts;
using OLab.Api.Models;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using OLab.TurkTalk.Endpoints.Utils;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task<DispatchedMessages> RegisterModeratorAsync(
    RegisterParticipantRequest payload)
  {
    DatabaseUnitOfWork dbUnitOfWork = null;

    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));

      TtalkConferenceTopic physTopic = null;
      TtalkTopicRoom physRoom = null;

      dbUnitOfWork = new DatabaseUnitOfWork(
        _logger,
        ttalkDbContext);

      var topicHandler = new ConferenceTopicHelper(_logger, _conference, dbUnitOfWork);
      var roomHandler = new TopicRoomHelper(_logger, topicHandler, dbUnitOfWork);

      // check if moderator is already known
      var physModerator =
        dbUnitOfWork.TopicParticipantRepository.GetModeratorBySessionId(payload.ContextId);

      // existing moderator
      if (physModerator != null)
      {
        physRoom =
          roomHandler.Get(physModerator.RoomId.Value);
        physTopic = physRoom.Topic;

        // update connectionId since it's probably changed
        dbUnitOfWork
          .TopicParticipantRepository
          .UpdateConnectionId( payload.ContextId, payload.ConnectionId );
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

        await dbUnitOfWork.TopicParticipantRepository.InsertAsync( physModerator );
        dbUnitOfWork.Save();

        var topicName = GetTopicNameFromQuestion(payload.QuestionId);

        physTopic =
          await topicHandler.GetCreateTopicAsync(
            _conference.Id,
            topicName);

        physRoom =
          await roomHandler.CreateRoomAsync(
            physTopic,
            physModerator);
      }

      // create and register signalr groups
      // against the connectionId

      topicHandler.RegisterModerator(
        MessageQueue,
        physTopic,
        physModerator);

      roomHandler.RegisterModerator(
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
