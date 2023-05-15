using Common.Utils;
using Microsoft.Extensions.Logging;
using OLab.TurkTalk.ParticipantSimulator;
using OLabWebAPI.Common.Contracts;
using OLabWebAPI.TurkTalk.Commands;
using OLabWebAPI.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OLabWebAPI.TurkTalk.BusinessObjects
{
  public class TopicAtrium
  {
    public IDictionary<string, Learner> AtriumLearners;
    private readonly OLabLogger _logger;
    public readonly Topic Topic;
    private Thread _contentScannerThread;

    public TopicAtrium(OLabLogger logger, Topic topic)
    {
      _logger = logger;
      Topic = topic;
      AtriumLearners = new ConcurrentDictionary<string, Learner>();

      //BuildAtriumScannerThread();
    }

    private void BuildAtriumScannerThread()
    {
      var proc = new TopicAtriumThread(_logger);
      ParameterizedThreadStart pts = proc.RunProc;

      _contentScannerThread = new Thread(pts);
      _contentScannerThread.IsBackground = true;

      _contentScannerThread.Start(this);
    }

    /// <summary>
    /// Get list of Participant
    /// </summary>
    /// <returns>List of Participant group strings</returns>
    public IList<Learner> GetContents()
    {
      return AtriumLearners.Values.ToList();
    }

    /// <summary>
    /// Test if Participant already exists in atrium
    /// </summary>
    /// <param name="name">Participant name</param>
    /// <returns>true, if exists</returns>
    public bool Contains(Participant participant)
    {
      var found = AtriumLearners.ContainsKey(participant.GetUniqueKey());
      _logger.LogDebug($"{participant.UserId}: in '{Topic.Name}' atrium? {found}");
      return found;
    }

    /// <summary>
    /// Get Participant from atrium
    /// </summary>
    /// <param name="name">Participant name</param>
    /// <returns>true, if exists</returns>
    public Learner Get(Participant participant)
    {
      var key = participant.GetUniqueKey();
      if (AtriumLearners.ContainsKey(key))
        return AtriumLearners[key];

      return null;
    }

    /// <summary>
    /// Remove Participant from atrium
    /// </summary>
    /// <param name="participantName">Participant name</param>
    internal bool Remove(Participant participant)
    {
      // search atrium by user id
      var foundInAtrium = AtriumLearners.ContainsKey(participant.GetUniqueKey());
      if (foundInAtrium)
      {
        AtriumLearners.Remove(participant.GetUniqueKey());
        _logger.LogDebug($"{participant.GetUniqueKey()}: remove from '{Topic.Name}' atrium");
      }
      else
        _logger.LogDebug($"{participant.GetUniqueKey()}: remove: not found in '{Topic.Name}' atrium");

      Dump();

      return foundInAtrium;
    }

    /// <summary>
    /// Remove connection id from atrium
    /// </summary>
    /// <param name="connectionId">Connection id to search for</param>
    internal bool Remove(string connectionId)
    {
      foreach (Learner item in AtriumLearners.Values)
      {
        if (item.ConnectionId == connectionId)
          return Remove(item);
      }

      return false;
    }

    /// <summary>
    /// Add/update Participant to atrium
    /// </summary>
    /// <param name="participant">Participant to add</param>
    /// <returns>true if Participant replaced (versus just added)</returns>
    public bool Upsert(Learner participant)
    {
      var replaced = false;

      _logger.LogDebug($"{participant.GetUniqueKey()}: upsert to '{Topic.Name}' atrium");

      // remove if already exists
      if (Contains(participant))
      {
        _logger.LogDebug($"{participant.GetUniqueKey()}: remove from '{Topic.Name}' atrium");
        AtriumLearners.Remove(participant.GetUniqueKey());
        replaced = true;
      }

      Add(participant);
      Dump();

      return replaced;
    }

    /// <summary>
    /// Add Participant to atrium
    /// </summary>
    /// <param name="participant">Participant to add</param>
    internal void Add(Learner participant)
    {
      _logger.LogDebug($"{participant.GetUniqueKey()}: add to '{Topic.Name}' atrium");

      // used for chronological order querying/sorting
      participant.ReferenceDate = DateTime.Now;

      AtriumLearners.Add(participant.GetUniqueKey(), participant);
    }

    private void Dump()
    {
      _logger.LogDebug($"'{Topic.Name}': atrium contents. Count: {AtriumLearners.Values.Count} ");
      if (AtriumLearners.Values.Count == 0)
        _logger.LogDebug($"  none");
      else
      {
        foreach (Learner item in AtriumLearners.Values.OrderBy(x => x.UserId))
          _logger.LogDebug($"  {item.CommandChannel} ({ConnectionIdUtils.Shorten(item.ConnectionId)})");
      }
    }

    internal bool IsDuplicateLearner(Participant participant)
    {
      return (AtriumLearners.Values.Any(x => (x.UserId == participant.UserId)));
    }
  }
}
