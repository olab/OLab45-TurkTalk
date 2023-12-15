using Dawn;
using DocumentFormat.OpenXml.Presentation;
using OLab.Api.Common.Contracts;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.Utils;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task RegisterAttendeeAsync(RegisterAttendeePayload payload)
  {
    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));
      Guard.Argument(payload.RoomName).NotNull("RoomName");

      var learner = new UserInfoEncoder().DecryptUser(
        _configuration.GetAppSettings().Secret,
        payload.UserKey);

      learner.ConnectionId = payload.ConnectionId;
      learner.RoomName = payload.RoomName;

      // get topic from conference
      var topic = await _conference.GetTopicAsync(learner.RoomName);

      // add learner to topic
      await topic.AddAttendeeAsync(payload.ContextId, learner);

    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "RegisterAttendeeAsync");
      throw;
    }
  }
}
