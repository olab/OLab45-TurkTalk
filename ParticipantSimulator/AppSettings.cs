using Microsoft.Build.Framework;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration

namespace OLab.TurkTalk.ParticipantSimulator
{
  public class MapTrail
  {
    public int GetDelayMs(Settings settings)
    {
      if (PauseMs != null)
        return PauseMs.GetDelayMs();

      return settings.GetDelayMs();
    }

    [JsonPropertyName("MapId")]
    public uint MapId { get; set; }

    [JsonPropertyName("PauseMs")]
    public PauseMs PauseMs { get; set; }

    [JsonPropertyName("NodeTrail")]
    public List<NodeTrail> NodeTrail { get; set; }
  }

  public partial class NodeTrail
  {
    public int GetDelayMs(MapTrail mapTrail)
    {
      if (PauseMs != null)
        return PauseMs.GetDelayMs();

      if (mapTrail.PauseMs != null)
        return mapTrail.PauseMs.GetDelayMs();

      return 10000;
    }

    [JsonPropertyName("NodeId")]
    public uint NodeId { get; set; }

    [JsonPropertyName("PauseMs")]
    public PauseMs PauseMs { get; set; }

    [JsonPropertyName("TurkTalkTrail")]
    public TurkTalkTrail TurkTalkTrail { get; set; }
  }

  public class Participant
  {
    public Participant()
    {
    }

    public MapTrail GetMapTrail(Settings settings)
    {
      if (MapTrail != null)
        return MapTrail;

      if (settings.MapTrail != null)
        return settings.MapTrail;

      throw new Exception("Missing MapTrail setting");
    }

    public int GetDelayMs(Settings settings)
    {
      if (PauseMs != null)
        return PauseMs.GetDelayMs();

      if (MapTrail != null && MapTrail.PauseMs != null)
        return MapTrail.GetDelayMs(settings);

      if (settings != null && settings.MapTrail != null && settings.MapTrail.PauseMs != null)
        return settings.MapTrail.GetDelayMs(settings);

      if (settings != null && settings.PauseMs != null)
        return settings.GetDelayMs();

      return 10000;
    }

    [JsonPropertyName("UserId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("Password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("PauseMs")]
    public PauseMs PauseMs { get; set; }

    [JsonPropertyName("MapTrail")]
    public MapTrail MapTrail { get; set; }
  }

  public partial class PauseMs
  {
    public static Random rnd = new Random();

    public int GetDelayMs( int min = 0, int max = 0 )
    {
      int sleepMs = 0;
      if ( min == 0 && max == 0 )
        sleepMs = rnd.Next(this.MinTimeMs, this.MaxTimeMs);
      else
        sleepMs = rnd.Next(min, max);

      return sleepMs;
    }

    [JsonPropertyName("MinTimeMs")]
    public int MinTimeMs { get; set; } = 0;

    [JsonPropertyName("MaxTimeMs")]
    public int MaxTimeMs { get; set; }
  }

  public class Settings
  {
    public Settings()
    {
      Participants = new List<Participant>();
    }

    public CancellationToken GetToken()
    {
      return CancelTokenSource.Token;
    }

    public int GetDelayMs()
    {
      if (PauseMs != null)
        return PauseMs.GetDelayMs();

      return 10000;
    }

    private readonly CancellationTokenSource CancelTokenSource = new CancellationTokenSource();

    [JsonPropertyName("ApiRetryCount")]
    public int ApiRetryCount{ get; set; } = 5;

    [JsonPropertyName("LogDirectory")]
    public string LogDirectory { get; set; } = string.Empty;

    [JsonPropertyName("PauseMs")]
    public PauseMs PauseMs { get; set; }

    [JsonPropertyName("SignalRHubUrl")]
    public string SignalRHubUrl { get; set; } = string.Empty;

    [JsonPropertyName("OLabRestApiUrl")]
    public string OLabRestApiUrl { get; set; } = string.Empty;

    [JsonPropertyName("MapTrail")]
    public MapTrail MapTrail { get; set; }

    [JsonPropertyName("ParticipantInfo")]
    public ParticipantInfo ParticipantInfo{ get; set; }

    [JsonPropertyName("ParticipantList")]
    public List<Participant> Participants { get; set; }
  }

  public class ParticipantInfo
  {
    [JsonPropertyName("UserIdPrefix")]
    public string UserIdPrefix { get; set; }

    [JsonPropertyName("Password")]
    public string Password { get; set; }

    [JsonPropertyName("NumUsers")]
    public int NumUsers { get; set; }

    [JsonPropertyName("StartsAt")]
    public int? StartsAt { get; set; }

    [JsonPropertyName("PauseMs")]
    public PauseMs PauseMs { get; set; }

    public int GetDelayMs(Settings settings)
    {
      if (PauseMs != null)
        return PauseMs.GetDelayMs();

      if (settings.PauseMs != null)
        return settings.PauseMs.GetDelayMs();

      return 10000;
    }
  }

  public class TurkTalkTrail
  {
    [JsonPropertyName("QuestionId")]
    public uint QuestionId { get; set; }

    [JsonPropertyName("RoomName")]
    public string RoomName { get; set; }

    [JsonPropertyName("MessageCount")]
    public int MessageCount { get; set; }

    [JsonPropertyName("PauseMs")]
    public PauseMs PauseMs { get; set; }

    public int GetDelayMs(NodeTrail nodeTrail)
    {
      if (PauseMs != null)
        return PauseMs.GetDelayMs();

      if (nodeTrail.PauseMs != null)
        return nodeTrail.PauseMs.GetDelayMs();

      return 10000;
    }
  }

}
