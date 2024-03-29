using Dawn;
using OLab.Access;
using OLab.TurkTalk.Data.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System.Security.Claims;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  /// <summary>
  /// Get (or create) participant by session id
  /// </summary>
  /// <param name="auth">OLabAuthorization</param>
  /// <param name="sessionId">Session Id</param>
  /// <param name="connectionId">Connection Id</param>
  /// <returns>TtalkTopicParticipant</returns>
  public async Task<TtalkTopicParticipant> GetCreateParticipantAsync(
    OLabAuthentication auth,
    string sessionId,
    string connectionId)
  {
    var physLearner =
      TopicHelper
      .ParticipantHelper
      .GetBySessionId(sessionId);

    if (physLearner == null)
    {
      physLearner = new TtalkTopicParticipant
      {
        SessionId = sessionId,
        TokenIssuer = auth.Claims["iss"],
        UserId = auth.Claims["id"],
        UserName = auth.Claims[ClaimTypes.Name],
        NickName = auth.Claims["name"],
        ConnectionId = connectionId
      };

      await TopicHelper
        .ParticipantHelper
        .InsertAsync(physLearner);
    }

    return physLearner;
  }

  public async Task<DispatchedMessages> RegisterLearnerAsync(
    RegisterParticipantRequest payload,
    CancellationToken cancellation)
  {
    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));

      // get existing, or create new topic
      var physTopic =
        await TopicHelper.GetCreateTopicAsync(
          _conference,
          payload.MapId,
          payload.QuestionId,
          cancellation);

      // get learner by session id
      var physLearner =
        TopicHelper.ParticipantHelper.GetBySessionId(payload.ContextId);

      // previously unauthenticated learner, or topic has changed
      if ((!physLearner.TopicId.HasValue) ||
           (physLearner.TopicId.HasValue && (physLearner.TopicId != physTopic.Id)))
      {
        physLearner.TopicId = physTopic.Id;
        physLearner = TopicHelper
          .ParticipantHelper
          .Update(physLearner);

        // commit topic update to participant
        TopicHelper.CommitChanges();
      }

      var physModeratorList =
        TopicHelper.ParticipantHelper.GetModerators(physTopic.Id);

      // create and add connection to learners session channel
      MessageQueue.EnqueueAddConnectionToGroupAction(
        physLearner.ConnectionId,
        physLearner.RoomLearnerSessionChannel);

      // force an atrium update
      if (physLearner.IsInTopicAtrium())
        await TopicHelper.BroadcastAtriumAddition(
          physTopic,
          physLearner,
          physModeratorList.Count,
          MessageQueue,
          cancellation);

      else
      {
        var physRoom =
          RoomHelper.Get(physLearner.RoomId);

        //TODO: handle re-connection to room
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
      TopicHelper.CommitChanges();
    }
  }
}
