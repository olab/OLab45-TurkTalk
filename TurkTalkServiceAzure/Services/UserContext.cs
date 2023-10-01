using Microsoft.AspNetCore.Http;
using OLab.Api.Data.Interface;
using OLab.Api.Data;
using OLab.Api.Model;
using OLab.Api.Utils;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using OLab.Common.Interfaces;
using Microsoft.Azure.Functions.Worker.Http;

#nullable disable

namespace OLab.TurkTalk.Service.Azure.Services;

public class UserContext : IUserContext
{
  public const string WildCardObjectType = "*";
  public const uint WildCardObjectId = 0;
  public const string NonAccessAcl = "-";
  public ClaimsPrincipal User;
  public Users OLabUser;

  private readonly HttpRequestData _httpRequest;
  private IEnumerable<Claim> _claims;
  private readonly OLabDBContext _dbContext;
  private readonly IOLabLogger _logger;
  protected IList<SecurityRoles> _roleAcls = new List<SecurityRoles>();
  protected IList<SecurityUsers> _userAcls = new List<SecurityUsers>();

  private IOLabSession _session;
  private string _role;
  private IList<string> _roles;
  private uint _userId;
  private string _userName;
  private string _ipAddress;
  private string _issuer;
  private readonly string _courseName;
  private string _accessToken;

  public IOLabSession Session
  {
    get => _session;
    set => _session = value;
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

  public string SessionId { get { return Session.GetSessionId(); } }

  public string CourseName { get { return _courseName; } }

  // default ctor, needed for services Dependancy Injection
  public UserContext()
  {

  }

  public UserContext(IOLabLogger logger, OLabDBContext dbContext)
  {
    _dbContext = dbContext;
    _logger = logger;
    Session = new OLabSession(_logger.GetLogger(), dbContext, this);
  }

  public UserContext(IOLabLogger logger, OLabDBContext context, HttpRequestData request)
  {
    _dbContext = context;
    _logger = logger;
    _httpRequest = request;

    Session = new OLabSession(_logger.GetLogger(), context, this);

    LoadHttpRequest();
  }

  /// <summary>
  /// Extract claims from token
  /// </summary>
  /// <param name="token">Bearer token</param>
  private static IEnumerable<Claim> ExtractTokenClaims(string token)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
    return securityToken.Claims;
  }

  protected virtual void LoadHttpRequest()
  {
    var sessionId = _httpRequest.Headers.GetValues("OLabSessionId").FirstOrDefault();
    if (!string.IsNullOrEmpty(sessionId) && sessionId != "null")
    {
      Session.SetSessionId(sessionId);
      if (!string.IsNullOrWhiteSpace(Session.GetSessionId()))
        _logger.LogInformation($"Found ContextId {Session.GetSessionId()}.");
    }

    IPAddress = _httpRequest.Headers.GetValues("X-Forwarded-Client-Ip").FirstOrDefault();
    if (string.IsNullOrEmpty(IPAddress))
      IPAddress = "<unknown>";

    _accessToken = _httpRequest.Headers.GetValues("Authorization").FirstOrDefault();
    _claims = ExtractTokenClaims(_accessToken);

    UserName = _claims.FirstOrDefault(c => c.Type == "name")?.Value;
    Role = _claims.FirstOrDefault(c => c.Type == "role")?.Value;
    ReferringCourse = _claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData)?.Value;

    // separate out multiple roles, make lower case, remove spaces, and sort
    _roles = Role.Split(',')
      .Select(x => x.Trim())
      .Select(x => x.ToLower())
      .OrderBy(x => x)
      .ToList();

    UserId = (uint)Convert.ToInt32(_claims.FirstOrDefault(c => c.Type == "id")?.Value);
    Issuer = _claims.FirstOrDefault(c => c.Type == "iss")?.Value;

    _roleAcls = _dbContext.SecurityRoles.Where(x => _roles.Contains(x.Name.ToLower())).ToList();

  }

  /// <summary>
  /// Test if have requested access to securable object
  /// </summary>
  /// <param name="requestedPerm">Request permissions (RWED)</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  public bool HasAccess(string requestedPerm, string objectType, uint? objectId)
  {
    var grantedCount = 0;

    //_logger.LogDebug($"ACL request: '{requestedPerm}' on '{objectType}({objectId})'");

    if (!objectId.HasValue)
      objectId = WildCardObjectId;

    for (var i = 0; i < requestedPerm.Length; i++)
      if (HasSingleAccess(requestedPerm[i], objectType, objectId))
        grantedCount++;

    return grantedCount == requestedPerm.Length;
  }

  /// <summary>
  /// Test if have single ACL access
  /// </summary>
  /// <param name="requestedPerm">Single-letter ACL to test for</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  private bool HasSingleAccess(char requestedPerm, string objectType, uint? objectId)
  {
    var rc = HasUserLevelAccess(requestedPerm, objectType, objectId);
    if (!rc)
      rc = HasRoleLevelAccess(requestedPerm, objectType, objectId);

    return rc;
  }

  /// <summary>
  /// Test if have single role-level ACL access
  /// </summary>
  /// <param name="requestedPerm">Single-letter ACL to test for</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  private bool HasRoleLevelAccess(char requestedPerm, string objectType, uint? objectId)
  {
    // test for explicit non-access to specific object type and id
    var acl = _roleAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     x.Acl == NonAccessAcl).FirstOrDefault();

    if (acl != null)
    {
      _logger.LogDebug($"{acl} ? true");
      return true;
    }

    // test for specific object type and id
    acl = _roleAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     x.Acl.Contains(requestedPerm)).FirstOrDefault();

    if (acl != null)
    {
      _logger.LogDebug($"{acl} ? true");
      return true;
    }

    // test for specific object type and all ids
    acl = _roleAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == WildCardObjectId &&
     x.Acl.Contains(requestedPerm)).FirstOrDefault();

    if (acl != null)
    {
      _logger.LogDebug($"{acl} ? true");
      return true;
    }

    // test for default any object, any id
    acl = _roleAcls.Where(x =>
     x.ImageableType == WildCardObjectType &&
     x.ImageableId == WildCardObjectId &&
     x.Acl.Contains(requestedPerm)).FirstOrDefault();

    if (acl != null)
    {
      _logger.LogDebug($"{acl} ? true");
      return true;
    }

    return false;
  }

  /// <summary>
  /// Test if have single user-level ACL access
  /// </summary>
  /// <param name="requestedPerm">Single-letter ACL to test for</param>
  /// <param name="objectType">Securable object type</param>
  /// <param name="objectId">(optional) securable object id</param>
  /// <returns>true/false</returns>
  private bool HasUserLevelAccess(char requestedPerm, string objectType, uint? objectId)
  {

    // test for explicit non-access to specific object type and id
    var acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     x.Acl == NonAccessAcl).FirstOrDefault();

    if (acl != null)
    {
      _logger.LogDebug($"{acl} ? false");
      return false;
    }

    // test for most specific object acl
    acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == objectId.Value &&
     x.Acl.Contains(requestedPerm)).FirstOrDefault();

    if (acl != null)
    {
      _logger.LogDebug($"{acl} ? true");
      return true;
    }

    // test for specific object type acl
    acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == WildCardObjectId &&
     x.Acl.Contains(requestedPerm)).FirstOrDefault();

    if (acl != null)
    {
      _logger.LogDebug($"{acl} ? true");
      return true;
    }

    // test for all for object type acl
    acl = _userAcls.Where(x =>
     x.ImageableType == objectType &&
     x.ImageableId == 0 &&
     x.Acl.Contains(requestedPerm)).FirstOrDefault();

    if (acl != null)
    {
      _logger.LogDebug($"{acl} ? true");
      return true;
    }

    // test for generic acl
    acl = _userAcls.Where(x =>
     x.ImageableType == WildCardObjectType &&
     x.ImageableId == 0 &&
     x.Acl.Contains(requestedPerm)).FirstOrDefault();

    if (acl != null)
    {
      _logger.LogDebug($"{acl} ? true");
      return true;
    }

    return false;
  }
}

