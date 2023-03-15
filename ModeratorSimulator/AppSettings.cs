using Microsoft.Build.Framework;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class MapTrail
{
  [JsonPropertyName("MapId")]
  public uint MapId { get; set; }

  [JsonPropertyName("PauseMs")]
  public PauseMs PauseMs { get; set; }

  [JsonPropertyName("NodeTrail")]
  public List<NodeTrail> NodeTrail { get; set; }

  public int GetDelayMs(Settings settings)
  {
    if (PauseMs != null)
      return PauseMs.GetDelayMs();

    return settings.GetDelayMs();
  }
}

public class Moderator
{
  [JsonPropertyName("UserId")]
  public string UserId { get; set; }

  [JsonPropertyName("Password")]
  public string Password { get; set; }

  [JsonPropertyName("PauseMs")]
  public PauseMs PauseMs { get; set; }

  [JsonPropertyName("MapTrail")]
  public MapTrail MapTrail { get; set; }

  public int GetDelayMs(Settings settings)
  {
    if (PauseMs != null)
      return PauseMs.GetDelayMs();

    if (settings != null && settings.PauseMs != null)
      return settings.GetDelayMs();

    return 10000;
  }
}

public class NodeTrail
{
  [JsonPropertyName("NodeId")]
  public uint NodeId { get; set; }

  [JsonPropertyName("TurkTalkTrail")]
  public TurkTalkTrail TurkTalkTrail { get; set; }
}

public class Participant
{
  [JsonPropertyName("UserId")]
  public string UserId { get; set; }

  [JsonPropertyName("AutoAccept")]
  public bool AutoAccept { get; set; }

  [JsonPropertyName("AutoRespond")]
  public bool AutoRespond { get; set; }
}

public class PauseMs
{
  private static Random rnd = new Random();

  [JsonPropertyName("MinTimeMs")]
  public int MinTimeMs { get; set; }

  [JsonPropertyName("MaxTimeMs")]
  public int MaxTimeMs { get; set; }

  public int GetDelayMs()
  {
    int sleepMs = rnd.Next(this.MinTimeMs, this.MaxTimeMs);
    return sleepMs;
  }
}

public class Settings
{
  [JsonPropertyName("LogDirectory")]
  public string LogDirectory { get; set; }

  [JsonPropertyName("PauseMs")]
  public PauseMs PauseMs { get; set; }

  [JsonPropertyName("SignalRHubUrl")]
  public string SignalRHubUrl { get; set; }

  [JsonPropertyName("OLabRestApiUrl")]
  public string OLabRestApiUrl { get; set; }

  [JsonPropertyName("Moderators")]
  public List<Moderator> Moderators { get; set; }

  public int GetDelayMs()
  {
    if (PauseMs != null)
      return PauseMs.GetDelayMs();

    return 10000;
  }
}

public class TurkTalkTrail
{
  [JsonPropertyName("QuestionId")]
  public int QuestionId { get; set; }

  [JsonPropertyName("RoomName")]
  public string RoomName { get; set; }

  [JsonPropertyName("MessageCount")]
  public string MessageCount { get; set; }

  [JsonPropertyName("AutoAccept")]
  public bool AutoAccept { get; set; }

  [JsonPropertyName("AutoRespond")]
  public bool AutoRespond { get; set; }

  [JsonPropertyName("PauseMs")]
  public PauseMs PauseMs { get; set; }

  [JsonPropertyName("Participants")]
  public List<Participant> Participants { get; set; }

  public int GetDelayMs(Settings settings, MapTrail mapTrail)
  {
    if (PauseMs != null)
      return PauseMs.GetDelayMs();

    if (mapTrail.PauseMs != null)
      return mapTrail.PauseMs.GetDelayMs();

    if (settings.PauseMs != null)
      return settings.PauseMs.GetDelayMs();

    return 10000;
  }
}

