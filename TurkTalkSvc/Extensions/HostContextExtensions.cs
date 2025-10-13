using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TurkTalkSvc.Extensions;

/// <summary>
/// Helper Function for HttpContext.
/// This will available to functioncontext as a extension methods.
/// </summary>
public static class HostContextExtentions
{
  /// <summary>
  /// Gets the headers for a function context
  /// </summary>
  /// <param name="hostContext"></param>
  /// <returns>headers as a dictionary</returns>
  public static Dictionary<string, string> GetHttpRequestHeaders(this HttpContext hostContext)
  {
    var headers = new Dictionary<string, string>();
    foreach (var item in hostContext.Request.Headers)
      headers.Add(item.Key.ToLowerInvariant(), item.Value);

    return headers;
  }

  /// <summary>
  /// This method extract requestdata from functioncontext.
  /// https://github.com/Azure/azure-functions-dotnet-worker/issues/414
  /// </summary>
  /// <param name="hostContext"></param>
  /// <returns>return HttpRequest.</returns>
  public static HttpRequest GetHttpRequest(this HttpContext hostContext)
  {
    return hostContext.Request;
  }

  /// <summary>
  /// Create response from function context and with specified object and status code.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="hostContext"></param>
  /// <param name="statusCode"></param>
  /// <param name="data"></param>
  /// <returns>return task.</returns>
  public static async Task CreateJsonResponse<T>(
    this HttpContext hostContext,
    System.Net.HttpStatusCode statusCode, T data)
  {
    var request = hostContext.GetHttpRequest();
    if (request != null)
    {
      var response = request.HttpContext.Response;
      response.StatusCode = (int)statusCode;

      await response.WriteAsJsonAsync(data);
    }
  }
}