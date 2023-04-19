using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OLabWebAPI.Common.Contracts;
using OLabWebAPI.Model;
using OLabWebAPI.TurkTalk.BusinessObjects;
using OLabWebAPI.TurkTalk.Commands;
using System;
using System.Threading.Tasks;

namespace OLabWebAPI.Services.TurkTalk
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
    public async Task RegisterModerator(uint mapId, uint nodeId, string roomName, bool isBot)
    {
      Room room = null;
      Moderator moderator = null;

      try
      {
        Guard.Argument(roomName).NotNull(nameof(roomName));

        moderator = new Moderator(roomName, Context);

        _logger.LogInformation($"RegisterModerator: '{roomName}', {isBot} ({ConnectionId.Shorten(Context.ConnectionId)}) IP Address: {this.Context.GetHttpContext().Connection.RemoteIpAddress}");

        room = _conference.GetCreateTopicRoom(moderator);

        // add room index to moderator info
        moderator.AssignToRoom(room.Index);
        _logger.LogInformation($"moderator: '{moderator}'");

        await room.AddModeratorAsync(moderator, mapId, nodeId);
      }
      catch (Exception ex)
      {
        _logger.LogError($"RegisterModerator exception: {ex.Message}");

        if ( ( room != null) && ( moderator != null ) )
        {
          room.Topic.Conference.SendMessage(
            Context.ConnectionId, 
            new ServerErrorCommand(Context.ConnectionId, ex.Message));
        }

      }
    }
  }
}
