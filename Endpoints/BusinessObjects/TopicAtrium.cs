using Dawn;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using System.Collections.Concurrent;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class TopicAtrium : ITopicAtrium
{
  public ConferenceTopic Topic { get; }
  private IDictionary<string, TopicParticipant> _atriumLearners;
  private SemaphoreSlim _contentsSemaphore = new SemaphoreSlim(1, 1);

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
  /// Get list of learners
  /// </summary>
  /// <returns>List of learners</returns>
  public async Task<IList<TopicParticipant>> GetLearnersAsync()
  {
    try
    {
      await SemaphoreLogger.WaitAsync(
        Logger,
        $"list",
      _contentsSemaphore);

      return _atriumLearners.Values.ToList();
    }
    finally
    {
      SemaphoreLogger.Release(
        Logger,
        $"list",
        _contentsSemaphore);
    }
  }

  /// <summary>
  /// Test if attendee already exists in atrium
  /// </summary>
  /// <param name="learner">Learner to look for</param>
  /// <param name="doWait">Use semaphore in this call</param>
  /// <returns>true, if exists</returns>
  public async Task<bool> ContainsAsync(
    TopicParticipant learner,
    bool doWait = true)
  {
    try
    {
      if (doWait)
        await SemaphoreLogger.WaitAsync(
          Logger,
          $"contains",
        _contentsSemaphore);

      var attendeeKey = learner.ToString();
      var found = _atriumLearners.ContainsKey(attendeeKey);
      Logger.LogInformation($"{attendeeKey}: in '{TopicName}' atrium? {found}");

      return found;
    }
    finally
    {
      if (doWait)
        SemaphoreLogger.Release(
          Logger,
          $"contains",
          _contentsSemaphore);
    }

  }

  /// <summary>
  /// Get learner from atrium
  /// </summary>
  /// <param name="name">Participant name</param>
  /// <returns>true, if exists</returns>
  public async Task<TopicParticipant> Get(TopicParticipant learner)
  {
    try
    {
      await SemaphoreLogger.WaitAsync(
        Logger,
        $"get",
      _contentsSemaphore);

      var atriumUserKey = learner.ToString();
      if (_atriumLearners.TryGetValue(atriumUserKey, out var value))
        return value;

      return null;
    }
    finally
    {
      SemaphoreLogger.Release(
        Logger,
        $"get",
        _contentsSemaphore);
    }

  }

  /// <summary>
  /// Remove learner from atrium
  /// </summary>
  /// <param name="learner">Learner to remove</param>
  public async Task<bool> RemoveAsync(TopicParticipant learner)
  {
    try
    {
      await SemaphoreLogger.WaitAsync(
        Logger,
        $"remove",
      _contentsSemaphore);

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
    finally
    {
      SemaphoreLogger.Release(
        Logger,
        $"remove",
        _contentsSemaphore);
    }

  }

  /// <summary>
  /// Remove learner from atrium by connection id
  /// </summary>
  /// <param name="connectionId">Connection id to search for</param>
  /// <returns>true is removed from atrium</returns>
  public async Task<bool> RemoveAsync(string connectionId)
  {

    var learner = _atriumLearners.Values.FirstOrDefault(x => x.ConnectionId == connectionId);
    if (learner != null)
      return await RemoveAsync(learner);

    return false;

  }

  /// <summary>
  /// Add learner to atrium
  /// </summary>
  /// <param name="dtoLearner">Participant to add</param>
  /// <returns>true if added to atrium, else was already in atrium</returns>
  public async Task<bool?> AddLearnerAsync(
    TopicParticipant dtoLearner,
    DispatchedMessages messageQueue)
  {
    try
    {
      await SemaphoreLogger.WaitAsync(
        Logger,
        $"add",
      _contentsSemaphore);

      if (!await ContainsAsync(dtoLearner, false))
      {
        var atriumUserKey = dtoLearner.ToString();
        _atriumLearners.Add(atriumUserKey, dtoLearner);

        Logger.LogInformation($"assigned learner '{dtoLearner}' to topic '{TopicName}' atrium");

        // signal to learner 'new' add to atrium
        //messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
        //    Configuration,
        //    dtoLearner.RoomLearnerSessionChannel,
        //    TopicName,
        //    true));

        //// signal to topic moderators atrium update
        //messageQueue.EnqueueMessage(new AtriumUpdateMethod(
        //  Configuration,
        //  Topic.TopicModeratorsChannel,
        //  _atriumLearners.Values.OrderBy(x => x.NickName).ToList()));

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
    finally
    {
      SemaphoreLogger.Release(
        Logger,
        $"add",
        _contentsSemaphore);
    }

  }

  /// <summary>
  /// Load atrium users from topic participants
  /// </summary>
  public async Task LoadAsync(IList<TopicParticipant> participants)
  {
    try
    {
      await SemaphoreLogger.WaitAsync(
        Logger,
        $"load",
      _contentsSemaphore);

      var atriumAttendees = participants.Where(x => x.RoomId == 0).ToList();

      foreach (var item in atriumAttendees)
      {
        var atriumUserKey = item.ToString();
        Logger.LogDebug($"{atriumUserKey}: loaded into '{TopicName}' atrium");
        _atriumLearners.Add(atriumUserKey, item);
      }

      Dump();
    }
    finally
    {
      SemaphoreLogger.Release(
        Logger,
        $"load",
        _contentsSemaphore);
    }

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
