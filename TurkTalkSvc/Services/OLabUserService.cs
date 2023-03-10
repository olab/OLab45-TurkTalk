using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OLabWebAPI.Model;
using OLabWebAPI.Utils;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OLabWebAPI.Services
{
  public class OLabUserService : IUserService
  {
    public static int defaultTokenExpiryMinutes = 120;
    private readonly AppSettings _appSettings;
    private readonly OLabDBContext _context;
    private readonly ILogger _logger;
    private readonly IList<Users> _users;
    private static TokenValidationParameters _tokenParameters;

    public OLabUserService(ILogger logger, IOptions<AppSettings> appSettings, OLabDBContext context)
    {
      defaultTokenExpiryMinutes = OLabConfiguration.DefaultTokenExpiryMins;
      _appSettings = appSettings.Value;
      _context = context;
      _logger = logger;

      _users = _context.Users.OrderBy(x => x.Id).ToList();

      _logger.LogDebug($"appSetting aud: '{_appSettings.Audience}', secret: '{_appSettings.Secret[..4]}...'");

      _tokenParameters = SetupConfiguration(_appSettings);
    }


    public static TokenValidationParameters GetValidationParameters()
    {
      return _tokenParameters;
    }

    private static TokenValidationParameters SetupConfiguration(AppSettings appSettings)
    {
      var jwtIssuer = "moodle";
      var jwtAudience = appSettings.Audience;
      var signingSecret = appSettings.Secret;

      var securityKey =
        new SymmetricSecurityKey(Encoding.Default.GetBytes(signingSecret[..16]));

      var tokenParameters = new TokenValidationParameters
      {
        ValidateIssuer = false,
        ValidIssuers = new List<string> { jwtIssuer, appSettings.Issuer },

        ValidateAudience = true,
        ValidAudience = jwtAudience,

        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        ClockSkew = TimeSpan.Zero,

        // validate against existing security key
        IssuerSigningKey = securityKey
      };

      return tokenParameters;

    }

    /// <summary>
    /// Adds user to database
    /// </summary>
    /// <param name="newUser"></param>
    public void AddUser(Users newUser)
    {

    }

    /// <summary>
    /// Authenticate external-issued token
    /// </summary>
    /// <param name="model">Login model</param>
    /// <returns>Authenticate response, or null</returns>
    public AuthenticateResponse AuthenticateExternal(ExternalLoginRequest model)
    {
      return GenerateExternalJwtToken(model);
    }

    /// <summary>
    /// Authenticate user
    /// </summary>
    /// <param name="model">Login model</param>
    /// <returns>Authenticate response, or null</returns>
    public AuthenticateResponse Authenticate(LoginRequest model)
    {
      Users user = _users.SingleOrDefault(x => x.Username.ToLower() == model.Username.ToLower());

      // return null if user not found
      if (user != null)
      {
        if (ValidatePassword(model.Password, user))
        {
          // authentication successful so generate jwt token
          return GenerateJwtToken(user);
        }
      }

      return null;
    }

    /// <summary>
    /// Updates a user record with a new password
    /// </summary>
    /// <param name="user">Existing user record from DB</param>
    /// <param name="model">Change password request model</param>
    /// <returns></returns>
    public void ChangePassword(Users user, ChangePasswordRequest model)
    {
      var clearText = model.NewPassword;

      // add password salt, if it's defined
      if (!string.IsNullOrEmpty(user.Salt))
        clearText += user.Salt;

      var hash = SHA1.Create();
      var plainTextBytes = Encoding.ASCII.GetBytes(clearText);
      var hashBytes = hash.ComputeHash(plainTextBytes);

      user.Password = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Get all defined users
    /// </summary>
    /// <returns>Enumerable list of users</returns>
    public IEnumerable<Users> GetAll()
    {
      return _users;
    }

    /// <summary>
    /// Get user by Id
    /// </summary>
    /// <param name="id">User id</param>
    /// <returns>User record</returns>
    public Users GetById(int id)
    {
      return _users.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// Get user by name
    /// </summary>
    /// <param name="userName">User name</param>
    /// <returns>User record</returns>
    public Users GetByUserName(string userName)
    {
      return _users.FirstOrDefault(x => x.Username.ToLower() == userName.ToLower());
    }

    /// <summary>
    /// Validate user password
    /// </summary>
    /// <param name="clearText">Password</param>
    /// <param name="user">Corresponding user record</param>
    /// <returns>true/false</returns>
    public bool ValidatePassword(string clearText, Users user)
    {
      if (!string.IsNullOrEmpty(user.Salt))
      {
        clearText += user.Salt;
        var hash = SHA1.Create();
        var plainTextBytes = Encoding.ASCII.GetBytes(clearText);
        var hashBytes = hash.ComputeHash(plainTextBytes);
        var localChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        return localChecksum == user.Password;
      }

      return false;
    }

    private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime FromUnixTime(long unixTime)
    {
      return epoch.AddSeconds(unixTime);
    }

    /// <summary>
    /// Generate JWT token from external one
    /// </summary>
    /// <param name="model">token payload</param>
    /// <returns>AuthenticateResponse</returns>
    private AuthenticateResponse GenerateExternalJwtToken(ExternalLoginRequest model)
    {
      var handler = new JwtSecurityTokenHandler();
      TokenValidationParameters tokenParameters = GetValidationParameters();

      JwtSecurityToken readToken = handler.ReadJwtToken(model.ExternalToken);

      _logger.LogDebug($"External JWT Incoming token claims:");
      foreach (Claim claim in readToken.Claims)
        _logger.LogDebug($" {claim}");

      handler.ValidateToken(model.ExternalToken,
                            tokenParameters,
                            out SecurityToken validatedToken);

      var jwtToken = (JwtSecurityToken)validatedToken;

      Users user = new Users();

      user.Username = $"{jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name").Value}";
      user.Role = $"{jwtToken.Claims.FirstOrDefault(x => x.Type == "role").Value}";
      user.Nickname = $"{jwtToken.Claims.FirstOrDefault(x => x.Type == "unique_name").Value}";
      user.Id = (uint)Convert.ToInt32( $"{jwtToken.Claims.FirstOrDefault(x => x.Type == "id").Value}" );

      var issuedBy = $"{jwtToken.Claims.FirstOrDefault(x => x.Type == "iss").Value}";
      return GenerateJwtToken(user, issuedBy);
    }

    /// <summary>
    /// Generate JWT token
    /// </summary>
    /// <param name="user">User record from database</param>
    /// <returns>AuthenticateResponse</returns>
    /// <remarks>https://duyhale.medium.com/generate-short-lived-symmetric-jwt-using-microsoft-identitymodel-d9c2478d2d5a</remarks>
    private AuthenticateResponse GenerateJwtToken(Users user, string issuedBy = "olab" )
    {
      var securityKey =
        new SymmetricSecurityKey(Encoding.Default.GetBytes(_appSettings.Secret[..16]));

      var tokenDescriptor = new SecurityTokenDescriptor
      {
        Subject = new ClaimsIdentity(new Claim[]
        {
          new Claim(ClaimTypes.Name, user.Username.ToLower()),
          new Claim(ClaimTypes.Role, $"{user.Role}"),
          new Claim("name", user.Nickname),
          new Claim("sub", user.Username),
          new Claim("id", $"{user.Id}")
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        Issuer = issuedBy,
        Audience = _appSettings.Audience,
        SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
      };

      var tokenHandler = new JwtSecurityTokenHandler();
      SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
      var securityToken = tokenHandler.WriteToken(token);

      var response = new AuthenticateResponse();
      response.AuthInfo.Token = securityToken;
      response.AuthInfo.Refresh = null;
      response.Role = $"{user.Role}";
      response.UserName = user.Username;
      response.AuthInfo.Created = DateTime.UtcNow;
      response.AuthInfo.Expires =
        response.AuthInfo.Created.AddMinutes(defaultTokenExpiryMinutes);

      return response;
    }

    public string GenerateRefreshToken()
    {
      var randomNumber = new byte[32];
      using var rng = RandomNumberGenerator.Create();
      rng.GetBytes(randomNumber);
      return Convert.ToBase64String(randomNumber);
    }
  }
}