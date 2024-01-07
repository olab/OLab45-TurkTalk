using Dawn;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task<DispatchedMessages> RegisterLearnerAsync(
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

      var topicHandler = new ConferenceTopic(_logger, _conference, dbUnitOfWork);
      var roomHandler = new TopicRoom(_logger, topicHandler, dbUnitOfWork);

      // check if moderator is already known
      var physLearner =
        dbUnitOfWork.TopicParticipantRepository.GetLearnerBySessionId(payload.ContextId);

      // existing learner
      if (physLearner == null)
      {
        physRoom =
          await roomHandler.GetAsync(physLearner.RoomId.Value);
        physTopic =
          await topicHandler.GetAsync(physRoom.TopicId);

        // update connectionId since it's probably changed
        dbUnitOfWork
          .TopicParticipantRepository
          .UpdateConnectionId(payload.ContextId, payload.ConnectionId);
        dbUnitOfWork.Save();
      }

      // new learner
      else
      {
        physLearner = new TtalkTopicParticipant
        {
          SessionId = payload.ContextId,
          TokenIssuer = payload.UserToken.TokenIssuer,
          UserId = payload.UserToken.UserId,
          UserName = payload.UserToken.UserName,
          NickName = payload.UserToken.NickName,
          ConnectionId = payload.ConnectionId,
          SeatNumber = 0
        };

        await dbUnitOfWork.TopicParticipantRepository.InsertAsync(physLearner);
        dbUnitOfWork.Save();

        var topicName = GetTopicNameFromQuestion(payload.QuestionId);

        physTopic =
          await topicHandler.GetCreateTopicAsync(
            _conference.Id,
            topicName);

        // add learner to topic atrium
        await topicHandler.AddToAtriumAsync(
          physTopic, 
          physLearner);
      }

      return MessageQueue;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "RegisterLearnerAsync");
      throw;
    }
    finally
    {
      dbUnitOfWork.Save();
    }
  }
}
