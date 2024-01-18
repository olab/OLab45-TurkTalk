using Dawn;
using OLab.TurkTalk.Data.Models;
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

      // get existing, or create new topic
      physTopic =
        await topicHelper.GetCreateTopicAsync(
          _conference,
          payload.NodeId,
          payload.QuestionId);

      var physLearner =
        topicHelper.GetLearnerBySessionId(payload.ContextId);

      // check if participent session is already known to topic
      if (physLearner != null)
      {
        topicHelper
          .ParticipantHelper
          .UpdateParticipantConnectionId(
            payload.ContextId, 
            payload.ConnectionId);

        // if participant was already in atrium,
        // just force an atrium update
        if (physLearner.IsInAtrium())
          await topicHelper.BroadcastAtriumAddition(
            physTopic,
            physLearner,
            MessageQueue);

        else
        {
          physRoom =
            roomHelper.Get(physLearner.RoomId);

          //TODO: handle re-connection to room
        }
      }

      // learner not known to topic
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
          TopicId = physTopic.Id
        };

        await topicHelper
          .ParticipantHelper
          .InsertAsync(physLearner);

        // create and add connection to learners session channel
        MessageQueue.EnqueueAddConnectionToGroupAction(
          physLearner.ConnectionId,
          physLearner.RoomLearnerSessionChannel);

        // notify moderators of atrium change
        await topicHelper.BroadcastAtriumAddition(
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
