using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.TurkTalkSvc.Services;
using OLab.TurkTalkSvc.TurkTalk.BusinessObjects;
using System;
using System.Net;

namespace OLabWebAPI.Services.TurkTalk
{
  // [Route("olab/api/v3/turktalk")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public partial class TurkTalkHub : Hub
  {
    private readonly IOLabLogger _logger;
    private readonly Conference _conference;
    protected readonly OLabDBContext DbContext;

    public string ContextId { get; set; }
    public uint QuestionId { get; set; }
    public uint NodeId { get; private set; }
    public uint MapId { get; private set; }

    /// <summary>
    /// TurkTalkHub constructor
    /// </summary>
    /// <param name="logger">Dependancy-injected logger</param>
    public TurkTalkHub(IOLabLogger logger, OLabDBContext dbContext, Conference conference)
    {
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(dbContext).NotNull(nameof(dbContext));
      Guard.Argument(conference).NotNull(nameof(conference));

      _conference = conference ?? throw new ArgumentNullException(nameof(conference));
      _logger = logger;

      DbContext = dbContext;

      _logger.LogDebug($"TurkTalkHub ctor");
    }

    /// <summary>
    /// Broadcast message to all participants
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    public void BroadcastMessage(string sender, string message)
    {
      try
      {
        _logger.LogDebug($"Broadcast message received from '{sender}': '{message}'");
      }
      catch (Exception ex)
      {
        _logger.LogError($"BroadcastMessage exception: {ex.Message}");
      }
    }

    private IUserContext GetUserContext()
    {
      var request = Context.GetHttpContext().Request;

      //var accessToken = $"Bearer {Convert.ToString(Context.GetHttpContext().Request.Query["access_token"])}";
      //request.Headers.Add("Authorization", accessToken);

      return new UserContext(_logger, DbContext, request);
    }

    // Extract user IP
    public IPAddress GetIp(HubCallerContext context)
    {
      // Return the Context IP address
      if (context != null)
      {
        var httpContext = context.GetHttpContext();
        if (httpContext != null)
        {
          IPAddress clientIp;
          IPAddress.TryParse(httpContext.Request.Headers["cf-connecting-ip"], out clientIp);
          return clientIp;
        }
      }

      return null;
    }

  }
}
