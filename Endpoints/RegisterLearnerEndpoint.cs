using Dawn;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Azure.Functions.Worker;
using OLab.Api.Common.Contracts;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using OLab.TurkTalk.Endpoints.Utils;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task<DispatchedMessages> RegisterLearnerAsync(
    RegisterParticipantRequest payload)
  {
    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));

      var dtoLearner = new TopicParticipant(payload);

      // get topic from conference, create topic if not exist yet
      var topic = await _conference.GetTopicAsync(payload.QuestionId);

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
