using Dawn;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OLab.Access;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using TurkTalkSvc.Interface;
using TurkTalkSvc.Services;

namespace TurkTalkSvc.Middleware;

public class OLabAuthMiddleware
{
  private readonly RequestDelegate _next;
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger _logger;

  public OLabAuthMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    RequestDelegate next)
  {
    Guard.Argument( configuration ).NotNull( nameof( configuration ) );
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>( loggerFactory );
    _logger.LogInformation( "OLabAuthMiddleware created" );

    _config = configuration;
    _next = next;
  }

  public static void SetupServices(IServiceCollection services)
  {
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
    var sp = services.BuildServiceProvider();
#pragma warning restore ASP0000 
    var configuration = sp.GetService<IOLabConfiguration>();
    var parameters = OLabAuthentication.BuildTokenValidationObject( configuration );

    services.AddAuthentication( x =>
    {
      x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    } )
    .AddJwtBearer( options =>
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
          var path = context.HttpContext.Request.Path;

          var accessToken = OLabAuthentication.ExtractAccessToken(
            context.Request,
            path.Value.Contains( "/login" ) );

          if ( !string.IsNullOrEmpty( accessToken ) &&
            path.StartsWithSegments( "/turktalk" ) )
            // Read the token out of the query string
            context.Token = accessToken;
          return Task.CompletedTask;
        }
      };
    } );
  }

  public void AttachUserToContext(
    HttpContext httpContext,
    IUserService userService,
    string token)
  {
    try
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      tokenHandler.ValidateToken( token,
                                 OLabAuthentication.BuildTokenValidationObject( _config ),
                                 out var validatedToken );

      var jwtToken = (JwtSecurityToken)validatedToken;
      var issuedBy = jwtToken.Claims.FirstOrDefault( x => x.Type == "iss" ).Value;
      var userName = jwtToken.Claims.FirstOrDefault( x => x.Type == "unique_name" ).Value;
      var role = jwtToken.Claims.FirstOrDefault( x => x.Type == "role" ).Value;

      httpContext.Items[ "claims" ] = jwtToken.Claims;

      var nickName = "";
      if ( jwtToken.Claims.Any( x => x.Type == "name" ) )
        nickName = jwtToken.Claims.FirstOrDefault( x => x.Type == "name" ).Value;
      else
        nickName = userName;
      httpContext.Items[ "UserId" ] = nickName;

      var id = 0;
      if ( jwtToken.Claims.Any( x => x.Type == "id" ) )
        id = Convert.ToInt16( jwtToken.Claims.FirstOrDefault( x => x.Type == "id" ).Value );
      httpContext.Items[ "Id" ] = id;

      httpContext.Items[ "IssuedBy" ] = issuedBy;

    }
    catch
    {
      // do nothing if jwt validation fails
      // user is not attached to DbContext so request won't have access to secure routes
    }
  }

  public async Task InvokeAsync(
    HttpContext httpContext,
    OLabDBContext dbContext,
    IOLabConfiguration configuration,
    IUserService userService)
  {
    var token = OLabAuthentication.ExtractAccessToken( httpContext.Request );

    if ( token != null )
    {
      // build and inject the host context into the authorixation object
      var authorization = new OLabAuthorization(
        _logger,
        dbContext,
        configuration );

      AttachUserToContext( httpContext, userService, token );

      httpContext.Items.Add( "authorization", authorization );
    }

    await _next( httpContext );
  }
}