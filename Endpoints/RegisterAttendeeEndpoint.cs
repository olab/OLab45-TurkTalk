using Dawn;
using DocumentFormat.OpenXml.Presentation;
using OLab.Api.Common.Contracts;
using OLab.Common.Interfaces;
using OLab.Data.BusinessObjects;
using OLab.TurkTalk.Data.BusinessObjects;
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
    }
    catch (Exception)
    {

      throw;
    }
  }
}
