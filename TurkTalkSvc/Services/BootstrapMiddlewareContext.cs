using Dawn;
using Microsoft.AspNetCore.Http;
using OLab.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TurkTalkSvc.Services;

/// <summary>
/// Helper class to manage and extract information
/// from the Azure Function execution context.
/// </summary>
public class BootstrapMiddlewareContext
{
  public HttpRequest Request { get; private set; }
  public string Url { get; private set; }
  public HttpContext ExecutionContext { get; }
  public IDictionary<string, string> Headers { get; private set; }

  private readonly IOLabLogger _logger;
  private IOLabLogger GetLogger() { return _logger; }

  public static BootstrapMiddlewareContext CreateInjectInstance(
    HttpContext executionContext,
    IOLabLogger logger)
  {
    var context = new BootstrapMiddlewareContext( executionContext, logger );
    executionContext.Items.Add( nameof( BootstrapMiddlewareContext ), context );
    return context;
  }

  public BootstrapMiddlewareContext(HttpContext executionContext, IOLabLogger logger)
  {
    ExecutionContext = executionContext;
    _logger = logger;

    try
    {
      GetLogger().LogInformation( $"BootstrapMiddlewareContext ctor" );

      Request = executionContext.Request;
      Headers = ExtractHeaders( Request );

      Url = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/{Request.Path}";
      GetLogger().LogInformation( $"  url: {Url}" );

    }
    catch ( Exception ex )
    {
      GetLogger().LogError( ex, "BootstrapMiddlewareContext exception" );
      throw;
    }

  }

  /// <summary>
  /// Extracts headers from the given HttpRequestData and returns them as a dictionary.
  /// </summary>
  /// <param name="httpRequestData">The HttpRequestData containing the headers to extract.</param>
  /// <returns>A dictionary containing the headers as key-value pairs.</returns>
  private IDictionary<string, string> ExtractHeaders(HttpRequest httpRequest)
  {
    var flatHeaderDict = new Dictionary<string, string>();
    foreach ( var header in httpRequest.Headers )
      flatHeaderDict.Add( header.Key.ToLower(), header.Value.First() );

    //GetLogger().LogInformation( $"found {flatHeaderDict.Count} headers" );
    //foreach ( var header in flatHeaderDict )
    //  GetLogger().LogInformation( $"  header: {header.Key} = {header.Value}" );

    return flatHeaderDict;
  }

  /// <summary>
  /// Retrieves the value of a specified header from the request headers.
  /// </summary>
  /// <param name="key">The key of the header to retrieve.</param>
  /// <param name="isRequired">Indicates whether the header is required. If true, an exception is thrown if the header is not found.</param>
  /// <returns>The value of the specified header if found; otherwise, an empty string if the header is not required and not found.</returns>
  /// <exception cref="Exception">Thrown if the header is required and not found.</exception>
  public string GetHeader(string key, bool isRequired = true)
  {
    if ( Headers.TryGetValue( key.ToLower(), out var value ) )
      return value;

    if ( isRequired )
      throw new Exception( $"header value '{key}' does not exist" );

    return string.Empty;
  }

}