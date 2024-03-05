using Dawn;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task<DispatchedMessages> RegisterModeratorAsync(
    RegisterParticipantRequest payload,
    CancellationToken cancellation)
  {
    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));

      TtalkTopicRoom physRoom = null;

      // get existing, or create new topic
      var physTopic =
        await TopicHelper.GetCreateTopicAsync(
          _conference,
          payload.MapId,
          payload.QuestionId,
          cancellation);

      // check if moderator is already known
      var physModerator =
        TopicHelper.ParticipantHelper.GetBySessionId(payload.ContextId);

      // existing moderator
      if (physModerator != null)
      {
        physRoom =
          RoomHelper.Get(physModerator.RoomId.Value);
        physTopic = physRoom.Topic;

        // update connectionId since it's probably changed
        TopicHelper
          .ParticipantHelper
          .UpdateParticipantConnectionId(payload.ContextId, payload.ConnectionId);
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

        await TopicHelper.ParticipantHelper.InsertAsync(physModerator);

        physTopic =
          await TopicHelper.GetCreateTopicAsync(
            _conference, 
            payload.NodeId,
            payload.QuestionId,
            cancellation);

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
        physModerator,
        cancellation);

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
      TopicHelper.CommitChanges();
    }
  }
}
