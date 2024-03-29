using Microsoft.AspNetCore.Http;
using OLab.Access;
using OLab.Api.Data;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace OLab.Api.Services
{
  public class UserContext : IUserContext
  {
    public const string WildCardObjectType = "*";
    public const uint WildCardObjectId = 0;
    public const string NonAccessAcl = "-";
    public ClaimsPrincipal User;
    public Users OLabUser;

    private readonly HttpContext _httpContext;
    private readonly HttpRequest _httpRequest;
    private IEnumerable<Claim> _claims;
    private readonly OLabDBContext _dbContext;
    private readonly IOLabLogger _logger;
    protected IList<SecurityRoles> _roleAcls = new List<SecurityRoles>();
    protected IList<SecurityUsers> _userAcls = new List<SecurityUsers>();

    protected string _sessionId;
    private string _role;
    private IList<string> _roles;
    private uint _userId;
    private string _userName;
    private string _ipAddress;
    private string _issuer;
    //private readonly string _courseName;
    private string _accessToken;

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

    public string SessionId
    {
      get { return _sessionId; }
      set { _sessionId = value; }
    }

    //public string CourseName { get { return _courseName; } }
    public string CourseName { get { return null; } }

    public IList<string> UserRoles { get { return _roles; } }

    // default ctor, needed for services Dependancy Injection
    public UserContext()
    {

    }

    public UserContext(IOLabLogger logger, OLabDBContext dbContext)
    {
      _dbContext = dbContext;
      _logger = logger;
    }

    public UserContext(IOLabLogger logger, OLabDBContext olabDbContext, HttpRequest request)
    {
      _dbContext = olabDbContext;
      _logger = logger;
      _httpRequest = request;

      LoadHttpRequest();
    }

    public UserContext(OLabLogger logger, OLabDBContext dbContext, HttpContext httpContext)
    {
      _dbContext = dbContext;
      _logger = logger;
      _httpContext = httpContext;

      LoadHttpContext();
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
      IPAddress = _httpRequest.Headers["X-Forwarded-Client-Ip"];
      if (string.IsNullOrEmpty(IPAddress))
        // request based requests need to get th eIPAddress using the context
        IPAddress = _httpRequest.HttpContext.Connection.RemoteIpAddress.ToString();

      _accessToken = OLabAuthentication.ExtractAccessToken(_httpRequest);
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

    protected virtual void LoadHttpContext()
    {
      IPAddress = _httpContext.Connection.RemoteIpAddress.ToString();

      var identity = (ClaimsIdentity)_httpContext.User.Identity;
      if (identity == null)
        throw new Exception($"Unable to establish identity from token");

      User = _httpContext.User;
      _claims = identity.Claims;

      UserName = User.FindFirst(ClaimTypes.Name).Value;
      ReferringCourse = User.FindFirst(ClaimTypes.UserData).Value;

      Issuer = User.FindFirst("iss").Value;
      UserId = (uint)Convert.ToInt32(User.FindFirst("id").Value);

      var Role = User.FindFirst(ClaimTypes.Role).Value;
      // separate out multiple roles, make lower case, remove spaces, and sort
      _roles = Role.Split(',')
        .Select(x => x.Trim())
        .Select(x => x.ToLower())
        .OrderBy(x => x)
        .ToList();

      _roleAcls = _dbContext.SecurityRoles.Where(x => _roles.Contains(x.Name.ToLower())).ToList();

      var ipAddress = _httpContext.Request.Headers["x-forwarded-for"].ToString();
      if (string.IsNullOrEmpty(ipAddress))
        ipAddress = _httpContext.Connection.RemoteIpAddress.ToString();
      _ipAddress = ipAddress;


      // test for a local user
      Users user = _dbContext.Users.FirstOrDefault(x => (x.Username == UserName) && (x.Id == UserId));
      if (user != null)
      {

        _logger.LogInformation($"Local user '{UserName}' found");

        OLabUser = user;
        UserId = user.Id;
        _userAcls = _dbContext.SecurityUsers.Where(x => x.UserId == UserId).ToList();

        // if user is anonymous user, add user access to anon-flagged maps
        if (OLabUser.Group == "anonymous")
        {
          var anonymousMaps = _dbContext.Maps.Where(x => x.SecurityId == 1).ToList();
          foreach (Maps item in anonymousMaps)
          {
            _userAcls.Add(new SecurityUsers
            {
              Id = item.Id,
              ImageableId = item.Id,
              ImageableType = Constants.ScopeLevelMap,
              Acl = "RX"
            });
          }
        }
      }

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

      //_logger.LogInformation($"ACL request: '{requestedPerm}' on '{objectType}({objectId})'");

      if (!objectId.HasValue)
        objectId = WildCardObjectId;

      for (var i = 0; i < requestedPerm.Length; i++)
      {
        if (HasSingleAccess(requestedPerm[i], objectType, objectId))
          grantedCount++;
        //else
        //  _logger.LogError($"User {UserId}/{Role} does not have '{requestedPerm[i]}' access to '{objectType}({objectId})'");
      }

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
      SecurityRoles acl = _roleAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == objectId.Value) &&
       (x.Acl == NonAccessAcl)).FirstOrDefault();

      if (acl != null)
      {
        _logger.LogInformation($"{acl} ? true");
        return true;
      }

      // test for specific object type and id
      acl = _roleAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == objectId.Value) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
      {
        _logger.LogInformation($"{acl} ? true");
        return true;
      }

      // test for specific object type and all ids
      acl = _roleAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == WildCardObjectId) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
      {
        _logger.LogInformation($"{acl} ? true");
        return true;
      }

      // test for default any object, any id
      acl = _roleAcls.Where(x =>
       (x.ImageableType == WildCardObjectType) &&
       (x.ImageableId == WildCardObjectId) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
      {
        _logger.LogInformation($"{acl} ? true");
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
      SecurityUsers acl = _userAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == objectId.Value) &&
       (x.Acl == NonAccessAcl)).FirstOrDefault();

      if (acl != null)
      {
        _logger.LogInformation($"{acl} ? false");
        return false;
      }

      // test for most specific object acl
      acl = _userAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == objectId.Value) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
      {
        _logger.LogInformation($"{acl} ? true");
        return true;
      }

      // test for specific object type acl
      acl = _userAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == WildCardObjectId) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
      {
        _logger.LogInformation($"{acl} ? true");
        return true;
      }

      // test for all for object type acl
      acl = _userAcls.Where(x =>
       (x.ImageableType == objectType) &&
       (x.ImageableId == 0) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
      {
        _logger.LogInformation($"{acl} ? true");
        return true;
      }

      // test for generic acl
      acl = _userAcls.Where(x =>
       (x.ImageableType == WildCardObjectType) &&
       (x.ImageableId == 0) &&
       x.Acl.Contains(requestedPerm)).FirstOrDefault();

      if (acl != null)
      {
        _logger.LogInformation($"{acl} ? true");
        return true;
      }

      return false;
    }
  }
}

