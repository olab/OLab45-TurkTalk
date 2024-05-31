using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OLab.Access;
using OLab.Api.Data;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.TurkTalk.BusinessObjects;
using OLab.Common.Interfaces;
using System;
using System.Net;

namespace OLab.Api.Services.TurkTalk
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
    public TurkTalkHub(
      IOLabLogger logger,
      OLabDBContext dbContext,
      Conference conference)
    {
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(dbContext).NotNull(nameof(dbContext));
      Guard.Argument(conference).NotNull(nameof(conference));

      _conference = conference ?? throw new ArgumentNullException(nameof(conference));
      _logger = logger;

      DbContext = dbContext;

      _logger.LogInformation($"TurkTalkHub ctor");
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
        _logger.LogInformation($"Broadcast message received from '{sender}': '{message}'");
      }
      catch (Exception ex)
      {
        _logger.LogError($"BroadcastMessage exception: {ex.Message}");
      }
    }

    /// <summary>
    /// ReadAsync the _authentication context from the host context
    /// </summary>
    /// <param name="hostContext">Function context</param>
    /// <returns>IOLabAuthentication</returns>
    /// <exception cref="Exception"></exception>
    [NonAction]
    protected IOLabAuthorization GetAuthorization(HttpContext hostContext)
    {
      // ReadAsync the item set by the middleware
      if (hostContext.Items.TryGetValue("usercontext", out var value) && value is IUserContext userContext)
      {
        _logger.LogInformation($"User context: {userContext}");

        var auth = new OLabAuthorization(_logger, DbContext);
        auth.ApplyUserContext(userContext);

        return auth;
      }

      throw new Exception("unable to get auth RequestContext");

    }

    /// <summary>
    /// Get the session from the host context
    /// </summary>
    /// <param name="hostContext">Function context</param>
    /// <param name="auth">IOLabAuthorization</param>
    /// <returns>IOLabSession</returns>
    /// <exception cref="Exception"></exception>
    [NonAction]
    private IOLabSession GetSession(HttpContext hostContext, IOLabAuthorization auth)
    {
      var request = hostContext.Request;

      var session = new OLabSession(_logger, DbContext, auth.UserContext);

      if (!request.Query.TryGetValue("mapId", out var mapId))
        throw new Exception("signalr hub missing mapId");
      session.SetMapId(Convert.ToUInt32(mapId));

      return session;
    }

    // Extract user IP
    [NonAction]
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
