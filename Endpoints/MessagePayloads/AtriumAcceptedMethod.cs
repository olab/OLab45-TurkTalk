﻿using Dawn;
using OLab.Common.Interfaces;
using System.Text;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class AtriumAcceptedMethod: TTalkMethod
{
  public string RoomName { get; set; }
  public bool WasAdded { get; set; }

  public AtriumAcceptedMethod(
    IOLabConfiguration configuration,
    string connectionId,
    string roomName,
    bool wasAdded ) : base(
      configuration,
      connectionId,
      "atriumaccepted")
  {
    Guard.Argument(roomName, nameof(roomName)).NotEmpty();
    Guard.Argument(connectionId, nameof(connectionId)).NotEmpty();
    Guard.Argument(roomName, nameof(roomName)).NotEmpty();

    RoomName = roomName;
    WasAdded = wasAdded;
  }

  public override object Arguments()
  {
    return this;
  }

  public override string ToString()
  {
    return $"{RoomName} {WasAdded}";
  }
}
