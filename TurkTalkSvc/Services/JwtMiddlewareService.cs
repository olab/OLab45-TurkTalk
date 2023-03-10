using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging.Signing;
using OLabWebAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLabWebAPI.Services
{
  public abstract class JwtMiddlewareService
  {
    protected readonly RequestDelegate _next;
    protected static string _jwtIssuers;
    protected static string _jwtAudience;
    protected static string _signingSecret;
    public static TokenValidationParameters _tokenParameters;

    protected abstract void AttachUserToContext(HttpContext httpContext,
                                     IUserService userService,
                                     string token);

    public JwtMiddlewareService(RequestDelegate next)
    {
      _next = next;
    }

    public static TokenValidationParameters GetValidationParameters()
    {
      return _tokenParameters;
    }

    protected static void SetupConfiguration(IConfiguration config)
    {
      _jwtIssuers = config["AppSettings:Issuer"];
      _jwtAudience = config["AppSettings:Audience"];
      _signingSecret = config["AppSettings:Secret"];

      var validIssuers = new List<string>();
      var issuerParts = _jwtIssuers.Split(',');
      foreach (var issuerPart in issuerParts)
        validIssuers.Add(issuerPart.Trim());

      var securityKey =
        new SymmetricSecurityKey(Encoding.Default.GetBytes(_signingSecret[..16]));

      _tokenParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidIssuers = validIssuers,
        ValidateIssuerSigningKey = true,

        ValidateAudience = true,
        ValidAudience = _jwtAudience,

        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        ClockSkew = TimeSpan.Zero,

        // validate against existing security key
        IssuerSigningKey = securityKey
      };

    }

    protected static void SetupServices(IServiceCollection services, TokenValidationParameters parameters)
    {
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

            var accessToken = AccessTokenUtils.ExtractAccessToken(
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

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
      var token = AccessTokenUtils.ExtractAccessToken(context.Request, true);

      if (token != null)
        AttachUserToContext(context,
                            userService,
                            token);

      await _next(context);
    }
  }
}