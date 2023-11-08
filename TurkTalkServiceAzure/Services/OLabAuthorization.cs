using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using OLab.Api.Common;
using OLab.Api.Data.Interface;
using OLab.Api.Dto;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using IOLabAuthentication = OLab.Api.Data.Interface.IOLabAuthentication;

namespace OLab.TurkTalk.Service.Azure.Services;

public class OLabAuthorization : IOLabAuthentication
{
  private readonly IUserContext userContext;

  public OLabAuthorization(
    IOLabLogger logger,
    OLabDBContext dbContext,
    FunctionContext context
  )
  {
    userContext = new UserContext(logger, dbContext, context);
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