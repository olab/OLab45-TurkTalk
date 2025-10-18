using OLab.TurkTalk.ModeratorSimulator;
using System.Collections.Concurrent;

public class WorkerThreadParameter
{
#pragma warning disable CS8618
  public WorkerThreadParameter()
#pragma warning restore CS8618
  {
    Statuses = new ConcurrentBag<KeyValuePair<string, string>>();
    Moderator = new Moderator();
    Settings = new Settings();
  }

  public ConcurrentBag<KeyValuePair<string, string>> Statuses { get; set; }
  public Moderator Moderator { get; set; }
  public CountdownEvent CountdownEvent { get; internal set; }
  public Settings Settings { get; internal set; }
  public Random Rnd { get; internal set; }
  public NLog.ILogger Logger { get; internal set; }
}