using Dawn;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System.Collections.Concurrent;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class TopicAtrium : ITopicAtrium
{
  public ConferenceTopic Topic { get; }
  private IDictionary<string, TopicParticipant> _atriumLearners;

  public string TopicName { get; }
  public IOLabLogger Logger { get; }
  public IOLabConfiguration Configuration { get; }

  public TopicAtrium(
    string topicName,
    IOLabLogger logger,
    IOLabConfiguration configuration)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    TopicName = topicName;
    Logger = logger;
    Configuration = configuration;
    _atriumLearners = new ConcurrentDictionary<string, TopicParticipant>();
  }

  /// <summary>
  /// Get list of Participant
  /// </summary>
  /// <returns>List of Participant group strings</returns>
  public IList<TopicParticipant> GetLearners()
  {
    return _atriumLearners.Values.ToList();
  }

  /// <summary>
  /// Test if attendee already exists in atrium
  /// </summary>
  /// <param name="learner">Learner to look for</param>
  /// <returns>true, if exists</returns>
  public bool Contains(TopicParticipant learner)
  {
    var attendeeKey = learner.ToString();
    var found = _atriumLearners.ContainsKey(attendeeKey);
    Logger.LogInformation($"{attendeeKey}: in '{TopicName}' atrium? {found}");
    return found;
  }

  /// <summary>
  /// Get learner from atrium
  /// </summary>
  /// <param name="name">Participant name</param>
  /// <returns>true, if exists</returns>
  public TopicParticipant Get(TopicParticipant learner)
  {
    var atriumUserKey = learner.ToString();
    if (_atriumLearners.ContainsKey(atriumUserKey))
      return _atriumLearners[atriumUserKey];

    return null;
  }

  /// <summary>
  /// Remove learner from atrium
  /// </summary>
  /// <param name="learner">Learner to remove</param>
  public bool Remove(TopicParticipant learner)
  {
    var atriumUserKey = learner.ToString();

    // search atrium by user id
    var foundInAtrium = _atriumLearners.ContainsKey(atriumUserKey);
    if (foundInAtrium)
    {
      _atriumLearners.Remove(atriumUserKey);
      Logger.LogDebug($"{atriumUserKey}: remove from '{TopicName}' atrium");
    }
    else
      Logger.LogDebug($"{atriumUserKey}: remove: not found in '{TopicName}' atrium");

    Dump();

    return foundInAtrium;
  }

  /// <summary>
  /// Remove learner from atrium by connection id
  /// </summary>
  /// <param name="connectionId">Connection id to search for</param>
  /// <returns>true is removed from atrium</returns>
  public bool Remove(string connectionId)
  {
    var learner = _atriumLearners.Values.FirstOrDefault(x => x.ConnectionId == connectionId);
    if (learner != null)
      return Remove(learner);

    return false;
  }

  /// <summary>
  /// Add learner to atrium
  /// </summary>
  /// <param name="dtoLearner">Participant to add</param>
  /// <returns>true if added to atrium, else was already in atrium</returns>
  public bool? AddLearner(
    TopicParticipant dtoLearner,
    DispatchedMessages messageQueue)
  {
    if (!Contains(dtoLearner))
    {
      var atriumUserKey = dtoLearner.ToString();
      _atriumLearners.Add(atriumUserKey, dtoLearner);

      Logger.LogInformation($"assigned learner '{dtoLearner}' to topic '{TopicName}' atrium");

      // signal to learner 'new' add to atrium
      messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
          Configuration,
          dtoLearner.RoomLearnerSessionChannel,
          TopicName,
          true));

      // signal to topic moderators atrium update
      messageQueue.EnqueueMessage(new AtriumUpdateMethod(
        Configuration,
        Topic.TopicModeratorsChannel,
        _atriumLearners.Values.OrderBy(x => x.NickName).ToList()));

      return true;

    }
    else
    {
      // signal to learner 'existing' add to atrium
      messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
          Configuration,
          dtoLearner.RoomLearnerSessionChannel,
          TopicName,
          false));

      return false;
    }

  }

  /// <summary>
  /// Load atrium users from topic participants
  /// </summary>
  public void Load(IList<TopicParticipant> participants)
  {
    var atriumAttendees = participants.Where(x => x.RoomId == 0).ToList();

    foreach (var item in atriumAttendees)
    {
      var atriumUserKey = item.ToString();
      Logger.LogDebug($"{atriumUserKey}: loaded into '{TopicName}' atrium");
      _atriumLearners.Add(atriumUserKey, item as TopicParticipant);
    }

    Dump();
  }

  private void Dump()
  {
    Logger.LogDebug($"'{TopicName}': atrium contents. Count: {_atriumLearners.Values.Count} ");
    if (_atriumLearners.Values.Count == 0)
      Logger.LogDebug($"  none");
    else
    {
      foreach (var item in _atriumLearners.Values.OrderBy(x => x.UserId))
        Logger.LogDebug($"  {item}");
    }
  }
}
