using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OLab.Access;
using OLab.Api.Data.Exceptions;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

#nullable disable

namespace OLab.Api.Services;

public class UserContextService : UserContextBase
{
  public UserContextService(IOLabLogger logger, OLabDBContext dbContext) : base(logger, dbContext)
  {
  }

  public UserContextService(
    OLabDBContext dbContext,
    IOLabLogger logger,
    HttpContext httpContext) : base(logger, dbContext)
  {
    Guard.Argument(httpContext).NotNull(nameof(httpContext));


    LoadHttpContext(httpContext);
  }

  protected virtual void LoadHttpContext(HttpContext hostContext)
  {
    if (!hostContext.Items.TryGetValue("headers", out var headersObjects))
      throw new Exception("unable to retrieve headers from host context");

    var headers = (Dictionary<string, string>)headersObjects;

    if (headers.TryGetValue("OLabSessionId".ToLower(), out var sessionId))
    {
      if (!string.IsNullOrEmpty(sessionId) && sessionId != "null")
      {
        SessionId = sessionId;
        if (!string.IsNullOrWhiteSpace(SessionId))
          _logger.LogInformation($"Found sessionId {SessionId}.");
      }
    }

    if (!hostContext.Items.TryGetValue("claims", out var claimsObject))
      throw new Exception("unable to retrieve claims from host context");

    _claims = (IDictionary<string, string>)claimsObject;

    if (!_claims.TryGetValue(ClaimTypes.Name, out var nameValue))
      throw new Exception("unable to retrieve user name from token claims");


    IPAddress = hostContext.Connection.RemoteIpAddress.ToString();

    UserName = nameValue;

    ReferringCourse = _claims[ClaimTypes.UserData];

    if (!_claims.TryGetValue("iss", out var issValue))
      throw new Exception("unable to retrieve iss from token claims");
    Issuer = issValue;

    if (!_claims.TryGetValue("id", out var idValue))
      throw new Exception("unable to retrieve user id from token claims");
    UserId = (uint)Convert.ToInt32(idValue);

    if (!_claims.TryGetValue(ClaimTypes.Role, out var roleValue))
      throw new Exception("unable to retrieve role from token claims");
    Role = roleValue;

    if ((Issuer == "olab") && (UserId != 0))
    {
      var userPhys = dbContext.Users
        .Include("UserGroups")
        .Include("UserGroups.Group")
        .Include("UserGroups.Role")
        .FirstOrDefault(x => x.Id == UserId);

      if (userPhys == null)
        throw new OLabObjectNotFoundException("Users", UserId);

      UserRoles = userPhys.UserGroups.ToList();
    }
    else
      // separate out multiple roles, make lower case, remove spaces, and sort
      UserRoles = UserGroups.FromString(dbContext, roleValue);

  }

}

