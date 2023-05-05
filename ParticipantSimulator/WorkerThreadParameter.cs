using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog;
using OLab.TurkTalk.ParticipantSimulator;

public class WorkerThreadParameter
{
#pragma warning disable CS8618
    public WorkerThreadParameter()
#pragma warning restore CS8618
    {
    Statuses = new ConcurrentBag<KeyValuePair<string, string>>();
    Participant = new Participant();
    Settings = new Settings();
  }

  public ConcurrentBag<KeyValuePair<string, string>> Statuses { get; set; }
  public Participant Participant { get; set; }
  public CountdownEvent CountdownEvent { get; internal set; }
  public Settings Settings { get; internal set; }
  public Random Rnd { get; internal set; }
  public NLog.ILogger Logger { get; internal set; }
}