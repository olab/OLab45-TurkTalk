using Dawn;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using OLab.Api.Common.Contracts;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using OLab.TurkTalk.Endpoints.Utils;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task RegisterModeratorAsync(
    RegisterParticipantPayload payload)
  {
    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));

      var dtoModerator = new TopicParticipant(payload);

      // get topic from conference, create topic if not exist yet
      var topic = await _conference.GetTopicAsync(payload.QuestionId);      
      dtoModerator.TopicId = topic.Id;

      // add moderator to topic
      await topic.AddModeratorAsync(
        dtoModerator,
        MessageQueue);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "RegisterModeratorAsync");
      throw;
    }
  }
}
