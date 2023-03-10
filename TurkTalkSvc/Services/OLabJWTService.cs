using Microsoft.AspNetCore.Http;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace OLabWebAPI.Services
{
  public class OLabJWTService : JwtMiddlewareService
  {
    private static SymmetricSecurityKey _securityKey;

    public OLabJWTService(RequestDelegate next) : base(next)
    {
    }

    public static void Setup(IConfiguration config, IServiceCollection services)
    {
      _securityKey =
        new SymmetricSecurityKey(Encoding.Default.GetBytes(config["AppSettings:Secret"][..16]));
      SetupConfiguration(config);

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
          // attach user to DbContext on successful jwt validation
          Model.Users user = userService.GetByUserName(userName);
          httpContext.Items["User"] = user.Username;
          httpContext.Items["Role"] = $"{user.Role}";
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
  }
}