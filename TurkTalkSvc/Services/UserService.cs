using Dawn;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OLab.Api.Services;

public class UserService : IUserService
{
  public static int defaultTokenExpiryMinutes = 120;
  private readonly OLabDBContext _dbContext;
  private readonly IOLabConfiguration _config;
  private readonly IOLabLogger Logger;

  public bool IsValid { get; private set; }
  public bool UserName { get; private set; }
  public bool Role { get; private set; }

  public UserService(
    ILoggerFactory loggerFactory,
    IOLabConfiguration config,
    OLabDBContext context)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(context).NotNull(nameof(context));
    Guard.Argument(config).NotNull(nameof(config));

    _dbContext = context;
    _config = config;
    defaultTokenExpiryMinutes = config.GetAppSettings().TokenExpiryMinutes;

    Logger = OLabLogger.CreateNew<UserService>(loggerFactory);

    Logger.LogInformation($"UserService ctor");
    Logger.LogInformation($"appSetting aud: '{config.GetAppSettings().Audience}', secret: '{config.GetAppSettings().Secret[..4]}...'");
  }

  /// <summary>
  /// Authenticate user
  /// </summary>
  /// <param name="model">Login model</param>
  /// <returns>OLab user object, or null, if authenticated</returns>
  public Users Authenticate(LoginRequest model)
  {
    Guard.Argument(model, nameof(model)).NotNull();

    Logger.LogInformation($"Authenticating {model.Username}, ***{model.Password[^3..]}");
    var user = _dbContext.Users.SingleOrDefault(x => x.Username.ToLower() == model.Username.ToLower());

    if (user != null)
    {
      if (!ValidatePassword(model.Password, user))
        return null;
    }

    return user;
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
  /// ReadAsync all defined users
  /// </summary>
  /// <returns>Enumerable list of users</returns>
  public IEnumerable<Users> GetAll()
  {
    return _dbContext.Users.ToList();
  }

  /// <summary>
  /// ReadAsync user by Id
  /// </summary>
  /// <param name="id">User id</param>
  /// <returns>User record</returns>
  public Users GetById(int id)
  {
    return _dbContext.Users.FirstOrDefault(x => x.Id == id);
  }

  /// <summary>
  /// ReadAsync user by name
  /// </summary>
  /// <param name="userName">User name</param>
  /// <returns>User record</returns>
  public Users GetByUserName(string userName)
  {
    return _dbContext.Users.FirstOrDefault(x => x.Username.ToLower() == userName.ToLower());
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

  public async Task<List<AddUserResponse>> DeleteUsersAsync(List<AddUserRequest> items)
  {
    try
    {
      var responses = new List<AddUserResponse>();

      Logger.LogDebug($"DeleteUserAsync(items count '{items.Count}')");

      foreach (var item in items)
      {
        var response = await DeleteUserAsync(item);
        responses.Add(response);
      }

      return responses;
    }
    catch (Exception ex)
    {
      Logger.LogError($"DeleteUserAsync exception {ex.Message}");
      throw;
    }
  }

  public async Task<AddUserResponse> DeleteUserAsync(AddUserRequest userRequest)
  {
    var user = GetByUserName(userRequest.Username);
    if (user == null)
    {
      return new AddUserResponse
      {
        Username = userRequest.Username.ToLower(),
        Message = $"User does not exist"
      };
    }

    var physUser =
      await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == userRequest.Username);

    _dbContext.Users.Remove(physUser);
    await _dbContext.SaveChangesAsync();

    var response = new AddUserResponse
    {
      Username = physUser.Username,
      Message = "Deleted"
    };

    return response;
  }

  public async Task<List<AddUserResponse>> AddUsersAsync(List<AddUserRequest> items)
  {
    try
    {
      var responses = new List<AddUserResponse>();

      Logger.LogDebug($"AddUserAsync(items count '{items.Count}')");

      foreach (var item in items)
      {
        var response = await AddUserAsync(item);
        responses.Add(response);
      }

      return responses;
    }
    catch (Exception ex)
    {
      Logger.LogError($"AddUserAsync exception {ex.Message}");
      throw;
    }
  }

  public async Task<AddUserResponse> AddUserAsync(AddUserRequest userRequest)
  {
    var user = GetByUserName(userRequest.Username);
    if (user != null)
    {
      return new AddUserResponse
      {
        Username = userRequest.Username.ToLower(),
        Message = $"Already exists"
      };
    }

    var newUser = Users.CreateDefault(userRequest);
    var newPassword = newUser.Password;

    ChangePassword(newUser, new ChangePasswordRequest
    {
      NewPassword = newUser.Password
    });

    await _dbContext.Users.AddAsync(newUser);
    await _dbContext.SaveChangesAsync();

    var response = new AddUserResponse
    {
      Username = newUser.Username,
      Password = newPassword
    };

    return response;
  }

}