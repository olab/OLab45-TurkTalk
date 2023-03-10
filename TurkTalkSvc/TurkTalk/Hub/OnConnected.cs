using Common.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
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
    /// A connection was established with hubusing Microsoft.AspNetCore.SignalR;
    /// </summary>
    /// <returns></returns>
    public override Task OnConnectedAsync()
    {
      try
      {
        _logger.LogDebug($"OnConnectedAsync: '{ConnectionId.Shorten(Context.ConnectionId)}'.");

        HttpRequest request = Context.GetHttpContext().Request;

        var accessToken = $"Bearer {Convert.ToString(request.Query["access_token"])}";
        request.Headers.Add("Authorization", accessToken);

        var feature = Context.Features.Get<IHttpConnectionFeature>();
        _logger.LogInformation($"SignalR client connected with IP {feature.RemoteIpAddress}");

      }
      catch (Exception ex)
      {
        _logger.LogError($"OnConnectedAsync exception: {ex.Message}");
      }

      return base.OnConnectedAsync();
    }
  }
}
