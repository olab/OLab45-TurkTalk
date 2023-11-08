using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Service.Azure.Services;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.Middleware;

public abstract class JWTMiddleware : IFunctionsWorkerMiddleware
{
  protected static IOLabConfiguration Config;
  protected static IOLabLogger Logger;
  protected static TokenValidationParameters TokenValidation;

  public JWTMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory)
  {
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));

    Logger = OLabLogger.CreateNew<JWTMiddleware>(loggerFactory);

    Logger.LogInformation("JwtMiddleware created");

    //Config = new OLabConfiguration(configuration);
    Config = configuration;
    TokenValidation = BuildTokenValidation();
  }

  public abstract Task Invoke(
    FunctionContext context, 
    FunctionExecutionDelegate next);

  /// <summary>
  /// Builds token validation object
  /// </summary>
  /// <param name="configuration">App configuration</param>
  private static TokenValidationParameters BuildTokenValidation()
  {
    try
    {

      // get and extract the valid token issuers
      var jwtIssuers = Config.GetValue<string>("Issuer");
      var issuerParts = jwtIssuers.Split(',');
      var validIssuers = issuerParts.Select(x => x.Trim()).ToList();

      var jwtAudience = Config.GetValue<string>("Audience");
      var signingSecret = Config.GetValue<string>("Secret");

      var securityKey =
        new SymmetricSecurityKey(Encoding.Default.GetBytes(signingSecret[..40]));

      TokenValidation = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidIssuers = validIssuers,
        ValidateIssuerSigningKey = true,

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        ClockSkew = TimeSpan.Zero,

        // validate against existing security key
        IssuerSigningKey = securityKey
      };

      return TokenValidation;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex.Message);
      throw;
    }

  }
}
