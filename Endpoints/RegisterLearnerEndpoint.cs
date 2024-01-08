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
    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));

      TtalkConferenceTopic physTopic = null;
      TtalkTopicRoom physRoom = null;

      // check if moderator is already known
      var physLearner =
        dbUnitOfWork.TopicParticipantRepository.GetLearnerBySessionId(payload.ContextId);

      // existing learner
      if (physLearner != null)
      {
        physRoom =
          roomHandler.Get(physLearner.RoomId.Value);

        // update connectionId since it's probably changed
        dbUnitOfWork
          .TopicParticipantRepository
          .UpdateConnectionId(payload.ContextId, payload.ConnectionId);

        dbUnitOfWork.Save();
      }

      // new learner
      else
      {
        var topicName =
          GetTopicNameFromQuestion(payload.QuestionId);

        // get existing, or create new topic
        physTopic =
          await topicHandler.GetCreateTopicAsync(
            _conference.Id,
            topicName);

        // create and save new learner
        physLearner = new TtalkTopicParticipant
        {
          SessionId = payload.ContextId,
          TokenIssuer = payload.UserToken.TokenIssuer,
          UserId = payload.UserToken.UserId,
          UserName = payload.UserToken.UserName,
          NickName = payload.UserToken.NickName,
          ConnectionId = payload.ConnectionId,
          TopicId = physTopic.Id
        };

        await dbUnitOfWork
          .TopicParticipantRepository
          .InsertAsync(physLearner);

        dbUnitOfWork.Save();

        // create and add connection to learners session channel
        MessageQueue.EnqueueAddConnectionToGroupAction(
          physLearner.ConnectionId,
          physLearner.RoomLearnerSessionChannel);

        // notify moderators of atrium change
        await topicHandler.AddLearnerToAtriumAsync(
          physTopic,
          physLearner,
          MessageQueue);
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
