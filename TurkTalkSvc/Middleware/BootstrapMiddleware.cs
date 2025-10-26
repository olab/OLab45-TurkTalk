using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using System.Threading.Tasks;
using TurkTalkSvc.Services;


namespace TurkTalkSvc.Middleware;

/// <summary>
/// Middleware for exposing the execution context
/// </summary>
public class BootstrapMiddleware
{
  private readonly RequestDelegate _next;
  private readonly IOLabLogger _logger;

  public BootstrapMiddleware(
    IOLabConfiguration configuration,
    ILoggerFactory loggerFactory,
    RequestDelegate next)
  {
    Guard.Argument( loggerFactory ).NotNull( nameof( loggerFactory ) );

    _logger = OLabLogger.CreateNew<OLabAuthMiddleware>( loggerFactory );
    _logger.LogInformation( "BootstrapMiddleware created" );
    _next = next;
  }

  public async Task InvokeAsync(HttpContext executionContext)
  {
    BootstrapMiddlewareContext.CreateInjectInstance( executionContext, _logger );
    await _next( executionContext );
  }
}
