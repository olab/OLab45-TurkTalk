using Dawn;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  //public async Task<DispatchedMessages> RegisterLearnerAsync(
  //  RegisterParticipantRequest payload)
  //{
  //  try
  //  {
  //    Guard.Argument(payload).NotNull(nameof(payload));

  //    var physRoom = GetRoomFromQuestion(payload.QuestionId);

  //    // get topic from conference, using questionId
  //    var topic = await _conference.GetTopicAsync(physRoom, false);

  //    var dtoLearner = new TopicParticipant(payload);
  //    dtoLearner.TopicId = topic.Id;

  //    // add learner to topic
  //    await topic.AddLearnerAsync(
  //      dtoLearner,
  //      MessageQueue);

  //    return MessageQueue;
  //  }
  //  catch (Exception ex)
  //  {
  //    _logger.LogError(ex, "RegisterLearnerAsync");
  //    throw;
  //  }
  //}
}
