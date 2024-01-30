using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OLab.Api.TurkTalk.BusinessObjects;
using System;
using System.Threading.Tasks;

namespace OLab.Api.Services.TurkTalk
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TurkTalkHub : Hub
  {
    /// <summary>
    /// Register moderator to atrium
    /// </summary>
    /// <param name="moderatorName">Moderator's name</param>
    /// <param name="roomName">Atrium name</param>
    /// <param name="roomName">Topic id</param>
    /// <param name="isbot">Moderator is a bot</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task RoomClose(string roomName)
    {
      try
      {
        Guard.Argument(roomName).NotNull(nameof(roomName));

        _logger.LogInformation($"RoomClose: '{roomName}', ({ConnectionIdUtils.Shorten(Context.ConnectionId)})");

        // get or create a conference topic
        Topic topic = _conference.GetCreateTopic(roomName, false);

        if (topic != null)
          await topic.RemoveRoomAsync(roomName);
      }
      catch (Exception ex)
      {
        _logger.LogError($"RoomClose exception: {ex.Message}");
      }
    }
  }
}
