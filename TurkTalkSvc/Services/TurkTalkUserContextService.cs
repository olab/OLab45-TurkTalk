using Dawn;
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

    GetLogger().LogInformation($"Claims:");
    foreach (var item in jwtToken.Claims)
      GetLogger().LogInformation($" '{item.Type}'");

    GetLogger().LogInformation($"Headers:");
    foreach (var item in jwtToken.Header.Keys)
      GetLogger().LogInformation($" '{item}'");


    IPAddress = hostContext.Connection.RemoteIpAddress.ToString();

    GetLogger().LogInformation($"HttpContext items:");
    foreach (var item in hostContext.Items)
      GetLogger().LogInformation($" '{item.Key}'");

    if (!hostContext.Items.TryGetValue("headers", out var headersObjects))
      throw new Exception("unable to retrieve headers from host context");
    Headers = (Dictionary<string, string>)headersObjects;

    if (!hostContext.Items.TryGetValue("claims", out var claimsObject))
      throw new Exception("unable to retrieve claims from host context");
    Claims = (IDictionary<string, string>)claimsObject;

    LoadContext();
  }

}

