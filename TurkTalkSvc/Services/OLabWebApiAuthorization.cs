using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OLab.Api.Common;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using IOLabAuthentication = OLab.Api.Data.Interface.IOLabAuthentication;
using IUserContext = OLab.Api.Data.Interface.IUserContext;

namespace OLab.TurkTalkSvc.Services;

class OLabWebApiAuthorization : IOLabAuthentication
{
  private readonly OLabLogger logger;
  private readonly OLabDBContext context;
  private readonly HttpContext httpContext;
  private readonly IUserContext userContext;

  public OLabWebApiAuthorization(OLabLogger logger, OLabDBContext context, HttpContext httpContext)
  {
    this.logger = logger;
    this.context = context;
    this.httpContext = httpContext;
    userContext = new UserContext(logger, context, httpContext);

  }

  public IUserContext GetUserContext()
  {
    return userContext;
  }

  public IActionResult HasAccess(string acl, ScopedObjectDto dto)
  {
    // test if user has access to write to parent.
    if (dto.ImageableType == Constants.ScopeLevelMap)
      if (!HasAccess("W", Constants.ScopeLevelMap, dto.ImageableId))
        return OLabUnauthorizedResult.Result();
    if (dto.ImageableType == Constants.ScopeLevelServer)
      if (!HasAccess("W", Constants.ScopeLevelServer, dto.ImageableId))
        return OLabUnauthorizedResult.Result();
    if (dto.ImageableType == Constants.ScopeLevelNode)
      if (!HasAccess("W", Constants.ScopeLevelNode, dto.ImageableId))
        return OLabUnauthorizedResult.Result();

    return new NoContentResult();
  }

  public bool HasAccess(string acl, string objectType, uint? objectId)
  {
    return userContext.HasAccess(acl, objectType, objectId);
  }
}