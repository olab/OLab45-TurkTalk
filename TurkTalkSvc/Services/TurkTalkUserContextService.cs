using Dawn;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using OLab.Access;
using OLab.Api.Data;
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

public class TurkTalkUserContextService : UserContextService
{
  protected IDictionary<string, string> _claims;
  private readonly IOLabConfiguration configuration;
  private readonly HttpContext httpContext;
  private readonly IUserService userService;
  private readonly string token;
  protected string _sessionId;

  public TurkTalkUserContextService(IOLabLogger logger, OLabDBContext dbContext) : base(logger, dbContext)
  {
  }

  public TurkTalkUserContextService(
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    IOLabLogger logger,
    HttpContext httpContext,
    IUserService userService,
    string token) : base(logger, dbContext)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(httpContext).NotNull(nameof(httpContext));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    this.configuration = configuration;

    this.httpContext = httpContext;
    this.userService = userService;
    this.token = token;

    LoadHttpContext(this.httpContext);
  }

  protected virtual void LoadHttpContext(HttpContext hostContext)
  {
    var tokenHandler = new JwtSecurityTokenHandler();
    tokenHandler.ValidateToken(token,
                               OLabAuthentication.BuildTokenValidationObject(configuration),
                               out SecurityToken validatedToken);

    var jwtToken = (JwtSecurityToken)validatedToken;

    var dict = new Dictionary<string, string>();
    foreach (var claim in jwtToken.Claims)
      dict.TryAdd(claim.Type, claim.Value);

    SetClaims(dict);

    LoadContext();

    // if no role passed in, then we assume it's a local user
    if (GroupRoles.Count() == 0 )
    {
      var user = userService.GetByUserName(UserName);
      httpContext.Items["User"] = user.Username;
      httpContext.Items["Role"] = UserGrouproles.ListToString(user.UserGrouproles.ToList());
    }
    else
    {
      httpContext.Items["Role"] = UserGrouproles.ListToString(GroupRoles.ToList());
      httpContext.Items["User"] = UserName;
    }
  }

}

