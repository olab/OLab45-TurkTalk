using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using OLab.Api.Common.Exceptions;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Service.Azure.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.Middleware;

public class OLabAuthMiddleware : JWTMiddleware
{
  private readonly IUserService _userService;
  private readonly OLabDBContext _dbContext;
  private IReadOnlyDictionary<string, string> _headers;
  private IReadOnlyDictionary<string, object> _bindingData;
  private string _functionName;
  private HttpRequestData _httpRequestData;

  public OLabAuthMiddleware(
    IOLabConfiguration config,
    ILoggerFactory loggerFactory,
    IUserService userService,
    OLabDBContext dbContext) : base(config, loggerFactory)
  {
    Guard.Argument(userService).NotNull(nameof(userService));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    _userService = userService;
    _dbContext = dbContext;
  }


  public override async Task Invoke(
    FunctionContext functionContext,
    FunctionExecutionDelegate next)
  {
    Guard.Argument(functionContext).NotNull(nameof(functionContext));
    Guard.Argument(next).NotNull(nameof(next));

    try
    {
      _headers = functionContext.GetHttpRequestHeaders();
      _bindingData = functionContext.BindingContext.BindingData;
      _functionName = functionContext.FunctionDefinition.Name.ToLower();
      _httpRequestData = functionContext.GetHttpRequestData();

      Logger.LogInformation($"Middleware Invoke. function '{_functionName}'");

      // skip middleware for non-authenicated endpoints
      if (_functionName.ToLower().Contains("index"))
        await next(functionContext);

      // This is added pre-function execution, function will have access to this information
      // in the context.Items dictionary
      //functionContext.Items.Add("middlewareitem", "Hello, from middleware");

      var token = ExtractAccessToken(functionContext, true);
      if (token != null)
        AttachUserToContext(functionContext,
                            _userService,
                            token);

      await next(functionContext);

      //try
      //{
      //  var token = ExtractAccessToken(functionContext, true);

      //  if (!string.IsNullOrEmpty(token))
      //  {

      //    var claimsPrincipal = ValidateToken(functionContext, token);

      //    functionContext.Items.Add("headers", _headers);

      //    // convert and save claims collection to dictionary
      //    var claimsDictionary = new Dictionary<string, string>();
      //    foreach (var claim in claimsPrincipal.Claims)
      //      claimsDictionary.Add(claim.Type, claim.Value);
      //    functionContext.Items.Add("claims", claimsDictionary);

      //    var auth = new OLabAuthorization(Logger, _dbContext, functionContext);
      //    functionContext.Items.Add("auth", auth);

      //    // run the function
      //    await next(functionContext);

      //    // This happens after function execution. We can inspect the context after the function
      //    // was invoked
      //    if (functionContext.Items.TryGetValue("functionitem", out var value) && value is string message)
      //      Logger.LogInformation($"From function: {message}");
      //  }
      //  else
      //    await next(functionContext);

      //}
      //catch (OLabUnauthorizedException)
      //{
      //  // Unable to get token from headers
      //  await functionContext.CreateJsonResponse(HttpStatusCode.Unauthorized, new { Message = "Token is not valid." });
      //  Logger.LogError("token not provided in request");
      //  return;
      //}
      //catch (Exception ex)
      //{
      //  Logger.LogError($"function error: {ex.Message} {ex.StackTrace}");
      //  return;
      //}


    }
    catch (Exception ex)
    {
      await functionContext.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
      Logger.LogError($"server error: {ex.Message} {ex.StackTrace}");
      return;
    }

  }

  protected void AttachUserToContext(
    FunctionContext functionContext,
    IUserService userService,
    string token)
  {
    try
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      tokenHandler.ValidateToken(token,
                                 TokenValidation,
                                 out var validatedToken);

      var jwtToken = (JwtSecurityToken)validatedToken;
      var userName = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub").Value;
      var role = jwtToken.Claims.FirstOrDefault(x => x.Type == "role").Value;

      functionContext.Items.Add("Role", role);
      functionContext.Items.Add("User", userName);
    }
    catch
    {
      // do nothing if jwt validation fails
      // user is not attached to DbContext so request won't have access to secure routes
    }
  }

  private ClaimsPrincipal ValidateToken(FunctionContext context, string token)
  {
    try
    {
      Guard.Argument(context).NotNull(nameof(context));

      // Try to validate the token. Throws if the 
      // token cannot be validated.
      var tokenHandler = new JwtSecurityTokenHandler();
      var claimsPrincipal = tokenHandler.ValidateToken(
        token,
        TokenValidation,
        out var validatedToken);

      return claimsPrincipal;
    }
    catch (Exception ex)
    {
      Logger.LogError(ex.Message);
      throw new OLabUnauthorizedException();
    }
  }

  /// <summary>
  /// Gets the access token from the request
  /// </summary>
  /// <param name="logger">ILogger instance</param>
  /// <param name="context">Function context</param>
  /// <param name="token">(out) JWT token</param>
  /// <returns>true if token found</returns>
  private string ExtractAccessToken(FunctionContext context, bool allowAnonymous = false)
  {
    Guard.Argument(context).NotNull(nameof(context));

    var token = string.Empty;

    if (_headers.TryGetValue("authorization", out var authHeader))
    {
      if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
      {
        token = authHeader.Substring("Bearer ".Length).Trim();
        Logger.LogInformation("bearer token provided");
      }
    }

    if (string.IsNullOrEmpty(token) && !allowAnonymous)
      throw new OLabUnauthorizedException();

    return token;
  }

}