using Dawn;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task<DispatchedMessages> OnDisconnectedAsync(
    IOLabConfiguration configuration,
    OnDisconnectedRequest payload,
    CancellationToken cancellation)
  {
    Guard.Argument(configuration, nameof(configuration)).NotNull();
    Guard.Argument(payload, nameof(payload)).NotNull();

    var physParticipant =
      TopicHelper.ParticipantHelper.GetByConnectionId(payload.ConnectionId);

    if (physParticipant == null)
    {
      _logger.LogError($"unable to find disconnected user with connection id {payload.ConnectionId}");
      return MessageQueue;
    }

    var physTopic = await TopicHelper.GetAsync(physParticipant.TopicId, false);

    // if is in atrium, remove since participant was disconnected
    if (physParticipant.IsInTopicAtrium())
    {
      TopicHelper.ParticipantHelper.Remove(physParticipant);

      // notify moderators of atrium change
      await TopicHelper.SignalAtriumChangeAsync(
        physTopic,
        MessageQueue, 
        cancellation);
    }
    else
    {
      // get participant room, throw if not found
      var physRoom = TopicHelper.RoomHelper.Get(physParticipant.RoomId, false);

      // if is in room, and is a learner, signal room learner disconnected
      if (physParticipant.IsRoomLearner())
        RoomHelper.DisconnectLearner(
          physRoom,
          physParticipant,
          MessageQueue);

      // if is in room, and is a moderator, signal room moderator disconnected
      else if (physParticipant.IsModerator())
        RoomHelper.DisconnectModerator(
          physRoom,
          physParticipant,
          MessageQueue);
    }

    return MessageQueue;

  }
}
