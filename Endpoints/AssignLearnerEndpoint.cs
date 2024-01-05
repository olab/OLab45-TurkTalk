using Dawn;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task<DispatchedMessages> AssignLearnerAsync(
    AssignLearnerRequest payload)
  {
    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));

      var physRoom = GetRoomFromQuestion(payload.QuestionId);

      // get topic from conference, using questionId
      var topic = await _conference.GetTopicAsync(physRoom, false);

      // add learner to topic room
      topic.AssignLearnerToRoom(
        MessageQueue,
        payload.ModeratorSessionId,
        payload.LearnerSessionId,
        payload.SeatNumber);

      return MessageQueue;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "RegisterLearnerAsync");
      throw;
    }
  }
}
