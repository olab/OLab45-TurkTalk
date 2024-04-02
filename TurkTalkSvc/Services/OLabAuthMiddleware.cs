using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using OLab.Access;
using OLab.Access.Interfaces;
using OLab.Api.Common.Exceptions;
using OLab.Api.Extensions;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OLab.Api.Services;

public class OLabAuthMiddleware
{
  //private readonly IUserService _userService;
  private readonly RequestDelegate _next;
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;

  public OLabAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    RequestDelegate next)
  {
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>(loggerFactory);
    _logger.LogInformation("OLabAuthMiddleware created");

    _config = configuration;
    _next = next;
  }

  public static void SetupServices(IServiceCollection services)
  {
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
    var sp = services.BuildServiceProvider();
#pragma warning restore ASP0000 
    var configuration = sp.GetService<IOLabConfiguration>();
    var parameters = OLabAuthentication.BuildTokenValidationObject(configuration);

    services.AddAuthentication(x =>
    {
      x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.RequireHttpsMetadata = true;
      options.SaveToken = true;
      options.TokenValidationParameters = parameters;
      options.Events = new JwtBearerEvents
      {
        // this event fires on every failed token validation
        OnAuthenticationFailed = context =>
        {
          return Task.CompletedTask;
        },

        // this event fires on every incoming message
        OnMessageReceived = context =>
        {
          // If the request is for our SignalR hub based on
          // the URL requested then don't bother adding olab issued token.
          // SignalR has it's own
          PathString path = context.HttpContext.Request.Path;

          var accessToken = OLabAuthentication.ExtractAccessToken(
            context.Request,
            path.Value.Contains("/login"));

          if (!string.IsNullOrEmpty(accessToken) &&
            (path.StartsWithSegments("/turktalk")))
          {
            // Read the token out of the query string
            context.Token = accessToken;
          }
          return Task.CompletedTask;
        }
      };
    });
  }

  protected void AttachUserToContext(
    HttpContext httpContext,
    IUserService userService,
    string token)
  {
    try
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      tokenHandler.ValidateToken(token,
                                 OLabAuthentication.BuildTokenValidationObject(_config),
                                 out SecurityToken validatedToken);

      var jwtToken = (JwtSecurityToken)validatedToken;
      var issuedBy = jwtToken.Claims.FirstOrDefault(x => x.Type == "iss").Value;
      var userName = jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name").Value;
      var role = jwtToken.Claims.FirstOrDefault(x => x.Type == "role").Value;

      var nickName = "";
      if (jwtToken.Claims.Any(x => x.Type == "name"))
        nickName = jwtToken.Claims.FirstOrDefault(x => x.Type == "name").Value;
      else
        nickName = userName;
      httpContext.Items["UserId"] = nickName;

      var course = "olabinternal";
      if (jwtToken.Claims.Any(x => x.Type == "course"))
      {
        course = jwtToken.Claims.FirstOrDefault(x => x.Type == "course").Value;
        httpContext.Items["Course"] = course;
      }

      httpContext.Items["IssuedBy"] = issuedBy;

      // if no role passed in, then we assume it's a local user
      if (string.IsNullOrEmpty(role))
      {
        Users user = userService.GetByUserName(userName);
        httpContext.Items["User"] = user.Username;
        httpContext.Items["Role"] = $"{string.Join( ",", user.UserGroups.Select(x => x.Group.Name).ToList())}";
      }
      else
      {
        httpContext.Items["Role"] = role;
        httpContext.Items["User"] = userName;
      }

    }
    catch
    {
      // do nothing if jwt validation fails
      // user is not attached to DbContext so request won't have access to secure routes
    }
  }

  public async Task InvokeAsync(
    HttpContext context,
    IUserService userService)
  {
    var token = OLabAuthentication.ExtractAccessToken(
      context.Request,
      true);

    if (token != null)
      AttachUserToContext(context,
                          userService,
                          token);

    await _next(context);
  }
}