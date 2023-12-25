using Common.Utils;
using DocumentFormat.OpenXml.Spreadsheet;
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
  public IDictionary<string, TopicParticipant> AtriumLearners;
  public IOLabLogger Logger { get { return Topic.Conference.Logger; } }

  public TopicAtrium(ConferenceTopic topic)
  {
    Topic = topic;
    AtriumLearners = new ConcurrentDictionary<string, TopicParticipant>();
  }

  /// <summary>
  /// Get list of Participant
  /// </summary>
  /// <returns>List of Participant group strings</returns>
  public IList<TopicParticipant> GetContents()
  {
    return AtriumLearners.Values.ToList();
  }

  /// <summary>
  /// Test if attendee already exists in atrium
  /// </summary>
  /// <param name="attendee">Attendee to test</param>
  /// <returns>true, if exists</returns>
  public bool Contains(TopicParticipant attendee)
  {
    var attendeeKey = attendee.ToString();
    var found = AtriumLearners.ContainsKey(attendeeKey);
    Logger.LogInformation($"{attendeeKey}: in '{Topic.Name}' atrium? {found}");
    return found;
  }

  /// <summary>
  /// Get Participant from atrium
  /// </summary>
  /// <param name="name">Participant name</param>
  /// <returns>true, if exists</returns>
  public TopicParticipant Get(TopicParticipant attendee)
  {
    var atriumUserKey = attendee.ToString();
    if (AtriumLearners.ContainsKey(atriumUserKey))
      return AtriumLearners[atriumUserKey];

    return null;
  }

  /// <summary>
  /// Remove attendee from atrium
  /// </summary>
  /// <param name="attendee">Attendee name</param>
  internal bool Remove(TopicParticipant attendee)
  {
    var atriumUserKey = attendee.ToString();

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
  /// <returns>true is removed from atrium</returns>
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
  /// <returns>true if added to atrium, else was already in atrium</returns>
  internal async Task<bool?> AddAttendeeAsync(
    TopicParticipant attendee)
  {
    if (!Contains(attendee))
    {
      var atriumUserKey = attendee.ToString();
      AtriumLearners.Add(atriumUserKey, attendee);

      var newAttendee = new TtalkTopicParticipant
      {
        UserId = attendee.UserId.ToString(),
        UserName = attendee.UserName,
        TokenIssuer = attendee.TokenIssuer,
        SessionId = attendee.SessionId,
        TopicId = Topic.Id,
      };

      await Topic.Conference.TTDbContext.TtalkTopicParticipants.AddAsync(newAttendee);
      await Topic.Conference.TTDbContext.SaveChangesAsync();

      return true;

    }
    else
      return false;

  }

  private void Dump()
  {
    Logger.LogDebug($"'{Topic.Name}': atrium contents. Count: {AtriumLearners.Values.Count} ");
    if (AtriumLearners.Values.Count == 0)
      Logger.LogDebug($"  none");
    else
    {
      foreach (var item in AtriumLearners.Values.OrderBy(x => x.UserId))
        Logger.LogDebug($"  {item}");
    }
  }

  internal void Load(IList<TopicParticipant> value)
  {
    foreach (var item in value)
    {
      var atriumUserKey = item.ToString();
      Logger.LogDebug($"{atriumUserKey}: loaded into '{Topic.Name}' atrium");
      AtriumLearners.Add(atriumUserKey, item);
    }

    Dump();
  }
}
