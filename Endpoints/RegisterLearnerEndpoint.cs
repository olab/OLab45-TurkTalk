using Dawn;
using OLab.TurkTalk.Data.Contracts;
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

      var roomName = GetRoomNameFromQuestion(payload.QuestionId);

      // get topic from conference, using questionId, create topic if not exist yet
      var topic = await _conference.GetTopicAsync(roomName);

      var dtoLearner = new TopicParticipant(payload);
      dtoLearner.TopicId = topic.Id;

      // add learner to topic
      await topic.AddLearnerAsync(
        dtoLearner,
        MessageQueue);

      return MessageQueue;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "RegisterLearnerAsync");
      throw;
    }
  }
}
