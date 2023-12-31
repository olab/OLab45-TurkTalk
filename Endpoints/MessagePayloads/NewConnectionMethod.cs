﻿using Dawn;
using Grpc.Net.Client.Configuration;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.Utils;
using System.Security.Claims;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class NewConnectionMethod : TTalkMethod
{
  private readonly IOLabAuthentication _auth;
  public string UserKey { get; set; }

  public NewConnectionMethod(
    IOLabConfiguration configuration,
    string connectionId,
    IOLabAuthentication auth) : base(
      configuration,
      connectionId,
      "newConnection")
  {
    Guard.Argument(auth).NotNull(nameof(auth));

    _auth = auth;

    UserKey = new UserToken().EncryptToken(
      Configuration.GetAppSettings().Secret,
      _auth.Claims["id"],
      _auth.Claims[ClaimTypes.Name],
      _auth.Claims["name"],
      _auth.Claims["iss"]);
  }

  public override object Arguments()
  {
    return this;
  }

  public override string ToString()
  {
    return UserKey;
  }
}