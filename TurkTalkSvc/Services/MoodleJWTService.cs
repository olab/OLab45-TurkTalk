using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace OLabWebAPI.Services
{
  public class MoodleJWTService : JwtMiddlewareService
  {
    private static string _certificateFile;

    public MoodleJWTService(RequestDelegate next) : base(next)
    {
    }

    public static void Setup(IConfiguration config, IServiceCollection services)
    {
      _certificateFile = config["AppSettings:CertificateFile"];

      if (!File.Exists(_certificateFile))
        throw new FileNotFoundException("Certificate file not found");

      SetupConfiguration(config);

      var cert = new X509Certificate2(_certificateFile);
      SecurityKey key = new X509SecurityKey(cert);

      _tokenParameters.ValidateLifetime = true;
      _tokenParameters.IssuerSigningKey = key;

      SetupServices(services, GetValidationParameters());
    }

    protected override void AttachUserToContext(HttpContext httpContext,
                                                IUserService userService,
                                                string token)
    {
      try
      {
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.ValidateToken(token,
                                   GetValidationParameters(),
                                   out SecurityToken validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var userName = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub").Value;
        var role = jwtToken.Claims.FirstOrDefault(x => x.Type == "role").Value;

        httpContext.Items["Role"] = role;
        httpContext.Items["User"] = userName;
      }
      catch
      {
        // do nothing if jwt validation fails
        // user is not attached to DbContext so request won't have access to secure routes
      }
    }
  }
}