using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System.Collections.Concurrent;

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
  /// Add learner to atrium
  /// </summary>
  /// <param name="dtoLearner">Participant to add</param>
  /// <returns>true if added to atrium, else was already in atrium</returns>
  internal async Task<bool?> AddLearnerAsync(
    TopicParticipant dtoLearner,
    DispatchedMessages messageQueue)
  {
    if (!Contains(dtoLearner))
    {
      var atriumUserKey = dtoLearner.ToString();
      AtriumLearners.Add(atriumUserKey, dtoLearner);

      var newAttendee = new TtalkTopicParticipant
      {
        UserId = dtoLearner.UserId.ToString(),
        UserName = dtoLearner.UserName,
        TokenIssuer = dtoLearner.TokenIssuer,
        SessionId = dtoLearner.SessionId,
        TopicId = Topic.Id,
        ConnectionId = dtoLearner.ConnectionId
      };

      await Topic.Conference.TTDbContext.TtalkTopicParticipants.AddAsync(newAttendee);
      await Topic.Conference.TTDbContext.SaveChangesAsync();

      Logger.LogInformation($"assigned learner '{dtoLearner}' to topic '{Topic.Name}' atrium");

      // signal to learner 'new' add to atrium
      messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
          Topic.Conference.Configuration,
          dtoLearner.RoomLearnerSessionChannel,
          Topic,
          true));

      // signal to topic moderators atrium update
      messageQueue.EnqueueMessage(new AtriumUpdateMethod(
        Topic.Conference.Configuration,
        Topic.TopicModeratorsChannel,
        AtriumLearners.Values.OrderBy(x => x.NickName).ToList()));

      return true;

    }
    else
    {
      // signal to learner 'existing' add to atrium
      messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
          Topic.Conference.Configuration,
          dtoLearner.RoomLearnerSessionChannel,
          Topic,
          false));

      return false;
    }

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

  internal void Load()
  {
    var atriumAttendees = Topic.Attendees.Where(x => x.RoomId == 0).ToList();

    foreach (var item in atriumAttendees)
    {
      var atriumUserKey = item.ToString();
      Logger.LogDebug($"{atriumUserKey}: loaded into '{Topic.Name}' atrium");
      AtriumLearners.Add(atriumUserKey, item as TopicParticipant);
    }

    Dump();
  }
}
