using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OLab.Api.Common;
using System.Net;

namespace TurkTalkSvc.Extensions;

public static class HttpRequestExtensions
{
  public static ContentResult CreateResponse<T>(
    this HttpRequest request,
    OLabApiResult<T> apiResponse)
  {
    var content = new ContentResult
    {
      StatusCode = (int)apiResponse.ErrorCode,
      ContentType = "application/json",
      Content = JsonConvert.SerializeObject(apiResponse)
    };

    return content;
  }

  public static ContentResult CreateNoContentResponse(
    this HttpRequest request)
  {
    var content = new ContentResult
    {
      StatusCode = (int)HttpStatusCode.NoContent,
      ContentType = "application/json"
    };

    return content;
  }
}
