using Dawn;
using Microsoft.AspNetCore.Http;
using OLab.Access;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

#nullable disable

namespace TurkTalkSvc.Services;

public class AuthenticatedMiddlewareContext : AuthenticatedContext
{
  public static AuthenticatedMiddlewareContext CreateInjectInstance(HttpContext executionContext, IOLabLogger logger, OLabDBContext dbContext)
  {
    var context = new AuthenticatedMiddlewareContext( executionContext, logger, dbContext );
    executionContext.Items.Add( context.GetType().Name, context );
    return context;
  }

  public AuthenticatedMiddlewareContext(
    HttpContext executionContext,
    IOLabLogger logger,
    OLabDBContext dbContext) : base( logger, dbContext )
  {
    Guard.Argument( logger ).NotNull( nameof( logger ) );
    Guard.Argument( executionContext ).NotNull( nameof( executionContext ) );

    GetLogger().LogInformation( $"FunctionUserContext ctor" );

    var bootstrapContext =
      executionContext.Items[ nameof( BootstrapMiddlewareContext ) ] as BootstrapMiddlewareContext;

    LoadHostContext( bootstrapContext );
  }

  private string GetRequestIpAddress(HttpRequest req)
  {
    try
    {
      var headerDictionary = req.Headers.ToDictionary( x => x.Key, x => x.Value, StringComparer.Ordinal );
      var key = "x-forwarded-for";

      if ( headerDictionary.ContainsKey( key ) )
      {
        var headerValues = headerDictionary[ key ];
        var ipn = headerValues.FirstOrDefault()?.Split( new char[] { ',' } ).FirstOrDefault()?.Split( new char[] { ':' } ).FirstOrDefault();

        GetLogger().LogInformation( $"found ip address: {ipn}" );

        return ipn;
      }

    }
    catch ( Exception )
    {
      // eat all exceptions
    }

    return "<unknown>";
  }

  protected void LoadHostContext(BootstrapMiddlewareContext bootstrapContext)
  {
    var req = bootstrapContext.Request;
    IPAddress = GetRequestIpAddress( req );

    if ( !bootstrapContext.ExecutionContext.Items.TryGetValue( "claims", out var claimsObject ) )
      throw new Exception( "unable to retrieve claims from host context" );

    var claims = claimsObject as IList<Claim>;
    SetClaims( claims );

    var sessionId = bootstrapContext.GetHeader( HEADER_SESSIONID, false );
    if ( sessionId != string.Empty )
      if ( !string.IsNullOrEmpty( sessionId ) && sessionId != "null" )
      {
        SessionId = sessionId;
        if ( !string.IsNullOrWhiteSpace( SessionId ) )
          GetLogger().LogInformation( $"Found {HEADER_SESSIONID} '{SessionId}'." );
      }
    else
      GetLogger().LogWarning( $"no {HEADER_SESSIONID} provided" );

    LoadUserContext();

  }

}

