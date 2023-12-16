using Common.Utils;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using OLab.TurkTalk.Endpoints.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class TopicAtrium
{
  public ConferenceTopic Topic { get; }
  public IDictionary<string, AttendeePayload> AtriumLearners;
  public IOLabLogger Logger { get { return Topic.Conference.Logger; } }

  public TopicAtrium(ConferenceTopic topic)
  {
    Topic = topic;
    AtriumLearners = new ConcurrentDictionary<string, AttendeePayload>();
  }

  /// <summary>
  /// Get list of Participant
  /// </summary>
  /// <returns>List of Participant group strings</returns>
  public IList<AttendeePayload> GetContents()
  {
    return AtriumLearners.Values.ToList();
  }

  /// <summary>
  /// Test if Participant already exists in atrium
  /// </summary>
  /// <param name="attendee">Participant</param>
  /// <returns>true, if exists</returns>
  public bool Contains(AttendeePayload attendee)
  {
    var found = AtriumLearners.ContainsKey(GetUniqueKey(attendee));
    Logger.LogInformation($"{attendee.UserKey}: in '{Topic.Name}' atrium? {found}");
    return found;
  }

  /// <summary>
  /// Get Participant from atrium
  /// </summary>
  /// <param name="name">Participant name</param>
  /// <returns>true, if exists</returns>
  public AttendeePayload Get(AttendeePayload attendee)
  {
    var atriumUserKey = GetUniqueKey(attendee);
    if (AtriumLearners.ContainsKey(atriumUserKey))
      return AtriumLearners[atriumUserKey];

    return null;
  }

  /// <summary>
  /// Remove Participant from atrium
  /// </summary>
  /// <param name="participantName">Participant name</param>
  internal bool Remove(AttendeePayload attendee)
  {
    var atriumUserKey = GetUniqueKey(attendee);

    // search atrium by user id
    var foundInAtrium = AtriumLearners.ContainsKey(atriumUserKey);
    if (foundInAtrium)
    {
      AtriumLearners.Remove(atriumUserKey);
      Logger.LogDebug($"{atriumUserKey}: remove from '{Topic.Name}' atrium");
    }
    else
      Logger.LogDebug($"{atriumUserKey}: remove: not found in '{Topic.Name}' atrium");

    Dump();

    return foundInAtrium;
  }

  /// <summary>
  /// Remove attendee from atrium by connection id
  /// </summary>
  /// <param name="connectionId">Connection id to search for</param>
  internal bool Remove(string connectionId)
  {
    foreach (var item in AtriumLearners.Values)
    {
      if (item.ConnectionId == connectionId)
        return Remove(item);
    }

    return false;
  }

  /// <summary>
  /// Add Participant to atrium
  /// </summary>
  /// <param name="learner">Participant to add</param>
  /// <param name="messageQueue">TurkTalk message queue</param>
  internal bool AddAttendee(
    AttendeePayload attendee,
    TTalkMessageQueue messageQueue)
  {
    var atriumUserKey = GetUniqueKey(attendee);
    Logger.LogDebug($"{atriumUserKey}: add to '{Topic.Name}' atrium");

    if (!Contains(attendee))
    {
      // used for chronological order querying/sorting
      attendee.ReferenceDate = DateTime.UtcNow;
      AtriumLearners.Add(atriumUserKey, attendee);

      var newAtriumAttendee = new TtalkAtriumAttendee
      {
        TokenIssuer = attendee.UserToken.TokenIssuer,
        TopicId = Topic.Id,
        UserId = attendee.UserToken.UserId.ToString(),
        UserName = attendee.UserToken.UserName
      };

      Topic.Conference.TTDbContext.TtalkAtriumAttendees.Add(newAtriumAttendee);

      // signal 'new' add to atrium
      messageQueue.EnqueueMethod(new AtriumAcceptedMethod(
          Topic.Conference.Configuration,
          attendee.ConnectionId,
          Topic.Name,
          true));
    }
    else
      // signal 'resumption' of user in atrium
      messageQueue.EnqueueMethod(new AtriumAcceptedMethod(
          Topic.Conference.Configuration,
          attendee.ConnectionId,
          Topic.Name,
          false));

    return true;
  }

  private string GetUniqueKey(AttendeePayload payload)
  {
    return $"{payload.UserKey}/{payload.ContextId}";
  }

  private void Dump()
  {
    Logger.LogDebug($"'{Topic.Name}': atrium contents. Count: {AtriumLearners.Values.Count} ");
    if (AtriumLearners.Values.Count == 0)
      Logger.LogDebug($"  none");
    else
    {
      foreach (var item in AtriumLearners.Values.OrderBy(x => x.UserKey))
        Logger.LogDebug($"  {item.CommandChannel} ({item.UserKey})");
    }
  }

}
