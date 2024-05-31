using Dawn;
using Microsoft.AspNetCore.Http;
using OLab.Access;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

#nullable disable

namespace OLab.Api.Services;

public class UserContextService : IUserContext
{
  public const string WildCardObjectType = "*";
  public const uint WildCardObjectId = 0;
  public const string NonAccessAcl = "-";
  public Model.Users OLabUser;

  protected IDictionary<string, string> _claims;
  private readonly IOLabConfiguration configuration;
  private readonly OLabDBContext dbContext;
  private readonly IOLabLogger _logger;
  private readonly HttpContext httpContext;
  private readonly IUserService userService;
  private readonly string token;
  protected IList<GrouproleAcls> _roleAcls = new List<GrouproleAcls>();
  protected IList<UserAcls> _userAcls = new List<UserAcls>();

  protected string _sessionId;
  private string _role;
  private IList<UserGrouproles> _groupRoles;
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

  public IList<UserGrouproles> GroupRoles
  {
    get => _groupRoles;
    set => _groupRoles = value;
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
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabLogger logger,
    HttpContext httpContext,
    IUserService userService,
    string token)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(httpContext).NotNull(nameof(httpContext));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    this.configuration = configuration;

    this.dbContext = dbContext;
    _logger = logger;
    this.httpContext = httpContext;
    this.userService = userService;
    this.token = token;

    LoadHttpContext();
  }

  protected virtual void LoadHttpContext()
  {
    try
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      tokenHandler.ValidateToken(token,
                                 OLabAuthentication.BuildTokenValidationObject(configuration),
                                 out var validatedToken);

      var jwtToken = (JwtSecurityToken)validatedToken;
      var issuedBy = jwtToken.Claims.FirstOrDefault(x => x.Type == "iss").Value;
      var userName = jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name").Value;
      var role = jwtToken.Claims.FirstOrDefault(x => x.Type == "role").Value;
      var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "id").Value;

      var nickName = "";
      if (jwtToken.Claims.Any(x => x.Type == "name"))
        nickName = jwtToken.Claims.FirstOrDefault(x => x.Type == "name").Value;
      else
        nickName = userName;
      httpContext.Items["UserId"] =
        jwtToken.Claims.FirstOrDefault(x => x.Type == "id").Value;

      var course = "olabinternal";
      if (jwtToken.Claims.Any(x => x.Type == "course"))
      {
        course = jwtToken.Claims.FirstOrDefault(x => x.Type == "course").Value;
        httpContext.Items["Course"] = course;
        ReferringCourse = course;
      }

      Issuer = issuedBy;

      // if no role passed in, then we assume it's a local user
      if (string.IsNullOrEmpty(role))
      {
        var user = userService.GetByUserName(userName);
        httpContext.Items["User"] = user.Username;
        httpContext.Items["Role"] = UserGrouproles.ListToString(user.UserGrouproles.ToList());
      }
      else
      {
        httpContext.Items["Role"] = role;
        httpContext.Items["User"] = userName;
      }

      UserName = httpContext.Items["User"].ToString();
      Role = httpContext.Items["Role"].ToString();
      UserId = Convert.ToUInt32(userId);

    }
    catch
    {
      // do nothing if jwt validation fails
      // user is not attached to DbContext so request won't have access to secure routes
    }

  }
  public override string ToString()
  {
    return $"{UserId} {Issuer} {UserName} {Role} {IPAddress} {ReferringCourse}";
  }
}

