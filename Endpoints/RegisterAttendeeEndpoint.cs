using Dawn;
using DocumentFormat.OpenXml.Presentation;
using OLab.Api.Common.Contracts;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using OLab.TurkTalk.Endpoints.Utils;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task RegisterAttendeeAsync(
    AttendeePayload payload)
  {
    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));
      Guard.Argument(payload.RoomName).NotNull("RoomName");

      // decrypt the user token from the payload
      payload.RefreshUserToken(_configuration.GetAppSettings().Secret);

      // get topic from conference, create if not exist yet
      var topic = await _conference.GetTopicAsync(payload);

      // add learner to topic
      await topic.AddAttendeeAsync(
        payload,
        MessageQueue);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "RegisterAttendeeAsync");
      throw;
    }
  }
}
