using Dawn;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Model;
using OLabWebAPI.TurkTalk.Commands;
using OLabWebAPI.Common.Contracts;
using OLabWebAPI.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace OLabWebAPI.TurkTalk.BusinessObjects
{
  /// <summary>
  /// A instance of a topic (to handle when there are
  /// multiple 'rooms' for a topic)
  /// </summary>
  public class Room
  {
    private readonly Topic _topic;
    private int _index;
    private readonly ConcurrentList<Learner> _learners;

    private Moderator _moderator = null;

    public int Index
    {
      get { return _index; }
      private set { _index = value; }
    }

    public Moderator Moderator { get { return _moderator; } }
    public string Name { get { return $"{_topic.Name}/{Index}"; } }
    public bool IsModerated { get { return _moderator != null; } }
    protected ILogger Logger { get { return _topic.Logger; } }
    public Topic Topic { get { return _topic; } }


    public Room(Topic topic, int index)
    {
      Guard.Argument(topic).NotNull(nameof(topic));
      _topic = topic;
      _index = index;
      _learners = new ConcurrentList<Learner>(Logger);

      Logger.LogDebug($"New room '{Name}'");
    }

    /// <summary>
    /// Add participant to room
    /// </summary>
    /// <param name="learnerName">Learner user name</param>
    /// <param name="connectionId">Connection id</param>
    internal async Task<bool> AddLearnerAsync(Learner learner)
    {
      try
      {

        learner.AssignToRoom(_index);

        _learners.Lock();

        // test if duplicate learner logging in. If so, then
        // we need to reject the request.
        if (IsDuplicateLearner(learner))
        {
          _topic.Conference.SendMessage(
            learner.ConnectionId,
            new LearnerMessageCommand(
              new MessagePayload(
                learner,
                $"User is already logged in. Unable to connect.")));
          return false;
        }

        // associate Participant connection to participate group
        await _topic.Conference.AddConnectionToGroupAsync(learner);

        _learners.Add(learner);

        Logger.LogDebug($"Added participant {learner} to room '{Name}'");

        // if have moderator, notify that the participant has been
        // assigned to their room
        if (Moderator != null)
          _topic.Conference.SendMessage(
            new LearnerAssignmentCommand(Moderator, learner));

      }
      finally
      {
        _learners.Unlock();
      }

      return true;
    }

    /// <summary>
    /// Add moderator to room
    /// </summary>
    /// <param name="moderator">Moderator</param>
    /// <param name="mapId">Map id for topic</param>
    internal async Task AddModeratorAsync(Moderator moderator, uint mapId, uint nodeId)
    {
      // test if duplicate moderator logging in. If so, then
      // we need to reject the request.
      if (IsDuplicateModerator(moderator))
        throw new Exception($"'{moderator.UserId}' already logged in. Unable to create another session.");

      if (!IsModerated)
        _moderator = moderator;

      // add new moderator to moderators group (for atrium updates)
      await _topic.Conference.AddConnectionToGroupAsync(
        _topic.TopicModeratorsChannel,
        moderator.ConnectionId);

      // add moderator to its own group so it can receive messages
      await _topic.Conference.AddConnectionToGroupAsync(moderator);

      // get nodes from the current map that exist the current node
      IList<MapNodeListItem> mapNodes = GetExitMapNodes(mapId, nodeId);

      // notify moderator of room assignment
      _topic.Conference.SendMessage(
        new ModeratorAssignmentCommand(moderator, mapNodes));

      // notify moderator of atrium contents
      _topic.Conference.SendMessage(
        new AtriumUpdateCommand(
          moderator.CommandChannel,
          _topic.AtriumGetContents()));

      var learners = _learners.Items;

      // notify moderator of already assigned learners
      _topic.Conference.SendMessage(
        new LearnerListCommand(
          moderator.CommandChannel,
          learners));

      // notify all learners in room of
      // moderator (re)connection
      foreach (Learner learner in learners)
        _topic.Conference.SendMessage(
            new RoomAssignmentCommand(learner));
    }

    private IList<MapNodeListItem> GetExitMapNodes(uint mapId, uint nodeId)
    {
      var mapNodeList = new List<MapNodeListItem>();

      using (IServiceScope scope = _topic.Conference.ScopeFactory.CreateScope())
      {
        OLabDBContext dbContext = scope.ServiceProvider.GetRequiredService<OLabDBContext>();
        Logger.LogDebug($"Got dbContext");

        // get all destination nodes from the non-hidden map links that
        // start from the nodeId we are interested in
        var mapNodeIds = dbContext.MapNodeLinks
          .Where(x => x.NodeId1 == nodeId && x.MapId == mapId && !x.Hidden.Value)
          .Select(x => x.NodeId2)
          .ToList();

        var mapNodes = dbContext.MapNodes.Where(x => mapNodeIds.Contains(x.Id)).ToList();
        foreach (MapNodes mapNode in mapNodes)
          mapNodeList.Add(new MapNodeListItem { Id = mapNode.Id, Name = mapNode.Title });
      }

      return mapNodeList;
    }

    /// <summary>
    /// Signals a disconnection of a room Participant
    /// </summary>
    /// <param name="connectionId"></param>
    internal async Task RemoveParticipantAsync(Participant participant)
    {
      Logger.LogDebug($"Removing {participant.UserId} from room '{Name}'");

      // not a moderated room, return since there's 
      // nothing more to do
      if (!IsModerated)
      {
        Logger.LogInformation($"Room {Name} is not already moderated");
        return;
      }

      try
      {
        _learners.Lock();

        // test if Participant to remove is the moderator
        if (participant.UserId == _moderator.UserId)
          await RemoveModeratorAsync(participant);
        else
          RemoveLearner(participant);
      }
      finally
      {
        _learners.Unlock();
      }

    }

    private async Task RemoveModeratorAsync(Participant participant)
    {
      if (participant.RemoteIpAddress != _moderator.RemoteIpAddress)
      {
        Logger.LogDebug($"Participant '{participant.UserId}' is a moderator for room '{Name}' but the remoteIP does not match.");
        return;
      }

      Logger.LogDebug($"Participant '{participant.UserId}' is a moderator for room '{Name}'. removing all learners.");

      // notify all known learners in room of moderator disconnection
      // and add them back into the atrium
      foreach (Learner learner in _learners.Items)
      {
        RemoveLearner(learner, false);
        await _topic.AddToAtriumAsync(learner);
      }

      _learners.Clear();

      // the moderator has left the building
      _moderator = null;
    }

    private void RemoveLearner(Participant participant, bool instantRemove = true)
    {

      Learner serverParticipant = _learners.Items.FirstOrDefault(x => x.UserId == participant.UserId);
      if (serverParticipant != null)
      {
        // if user id isn't from the same address as the wxisting learner,
        // then ignore the remove.
        //if (participant.RemoteIpAddress != _moderator.RemoteIpAddress)
        //{
        //  Logger.LogDebug($"Participant '{participant.UserId}' is a learner for room '{Name}' but the remoteIP does not match.");
        //  return;
        //}

        Logger.LogDebug($"Participant '{participant.UserId}' is a participant for room '{Name}'. removing.");

        // build/set assumed command channel for participant
        var commandChannel = $"{_topic.Name}/{Learner.Prefix}/{serverParticipant.UserId}";

        // Participant is a participant, notify it's channel of disconnect
        _topic.Conference.SendMessage(
          new RoomUnassignmentCommand(
            commandChannel,
            serverParticipant));

        // remove participant from list if needing instant removal
        if (instantRemove)
          _learners.Remove(serverParticipant);
      }
      else
        Logger.LogError($"Participant '{participant.UserId}' is NOT participant for room '{Name}'.");

    }

    /// <summary>
    /// Test if Participant exists in room
    /// </summary>
    /// <param name="participant">Participant to look for</param>
    /// <returns>true/false</returns>
    internal bool ParticipantExists(Participant participant)
    {
      var found = false;

      try
      {
        _learners.Lock();
        found = _learners.Items.Any(x => x.UserId == participant.UserId);
      }
      finally
      {
        _learners.Unlock();
      }

      return found;
    }

    /// <summary>
    /// Test for duplicate moderator login (from another machine)
    /// </summary>
    /// <param name="testModerator">Moderator to test</param>
    /// <returns>true/false</returns>
    internal bool IsDuplicateModerator(Moderator testModerator)
    {
      if (!IsModerated)
      {
        Logger.LogDebug($"Not duplicate moderator '{testModerator.UserId}'.  Room has no existing moderator");
        return false;
      }

      if ((_moderator.UserId == testModerator.UserId) &&
           (_moderator.RemoteIpAddress != testModerator.RemoteIpAddress))
      {
        Logger.LogError($"Duplicate moderator '{testModerator.UserId}' login detected from different machine {testModerator.RemoteIpAddress}.");
        return true;
      }

      return false;
    }

    /// <summary>
    /// Test for duplicate learner login (from another machine)
    /// </summary>
    /// <param name="learner">Learner to test</param>
    /// <returns>true/false</returns>
    internal bool IsDuplicateLearner(Learner learner)
    {
      try
      {
        _learners.Lock();
        return _learners.Items.Any(
          x => (x.UserId == learner.UserId) && (x.RemoteIpAddress != learner.RemoteIpAddress)
        );
      }
      finally
      {
        _learners.Unlock();
      }

    }
  }
}