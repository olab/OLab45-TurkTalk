using Dawn;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OLab.Access;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;

#nullable disable

namespace OLab.Api.Services;

public class UserContextService : IUserContext
{
  public const string WildCardObjectType = "*";
  public const uint WildCardObjectId = 0;
  public const string NonAccessAcl = "-";
  public Users OLabUser;

  protected IDictionary<string, string> _claims;
  private readonly OLabDBContext dbContext;
  private readonly IOLabLogger _logger;
  protected IList<SecurityRoles> _roleAcls = new List<SecurityRoles>();
  protected IList<SecurityUsers> _userAcls = new List<SecurityUsers>();

  protected string _sessionId;
  private string _role;
  public IList<UserGroups> UserRoles { get; set; }
  private uint _userId;
  private string _userName;
  private string _ipAddress;
  private string _issuer;

  public string SessionId
  {
    get => _sessionId;
    set => _sessionId = value;
  }

  public string ReferringCourse
  {
    get => _role;
    set => _role = value;
  }

  public string Role
  {
    get => _role;
    set => _role = value;
  }

  public uint UserId
  {
    get => _userId;
    set => _userId = value;
  }

  public string UserName
  {
    get => _userName;
    set => _userName = value;
  }

  public string IPAddress
  {
    get => _ipAddress;
    set => _ipAddress = value;
  }

  public string Issuer
  {
    get => _issuer;
    set => _issuer = value;
  }
  string IUserContext.SessionId
  {
    get => _sessionId;
    set => _sessionId = value;
  }

  public UserContextService(IOLabLogger logger, OLabDBContext dbContext)
  {
    this.dbContext = dbContext;
    _logger = logger;
  }

  public UserContextService(
    OLabDBContext dbContext,
    IOLabLogger logger,
    HttpContext httpContext)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(httpContext).NotNull(nameof(httpContext));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    this.dbContext = dbContext;
    _logger = logger;

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

    UserRoles = dbContext.UserGroups.Where(x => x.UserId == UserId).ToList();

  }
  public override string ToString()
  {
    return $"{UserId} {Issuer} {UserName} {Role} {IPAddress} {ReferringCourse}";
  }
}

