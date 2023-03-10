using Dawn;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.TurkTalk.Commands;
using OLabWebAPI.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OLabWebAPI.TurkTalk.BusinessObjects
{
  /// <summary>
  /// Chat topic
  /// </summary>
  public class Topic
  {
    private readonly Conference _conference;
    private readonly ConcurrentList<Room> _rooms;

    private string _name;
    public string TopicModeratorsChannel;
    private TopicAtrium _atrium;

    private static readonly Mutex atriumMutex = new Mutex();
    public ILogger Logger { get { return _conference.Logger; } }

    public Conference Conference
    {
      get { return _conference; }
    }

    public string Name
    {
      get { return _name; }
      private set { _name = value; }
    }

    public IList<Room> Rooms
    {
      get { return _rooms.Items; }
    }


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="conference"></param>
    /// <param name="topicId"></param>
    public Topic(Conference conference, string topicId)
    {
      Guard.Argument(conference).NotNull(nameof(conference));

      _conference = conference;
      _rooms = new ConcurrentList<Room>(Logger);

      Name = topicId;
      _atrium = new TopicAtrium(Logger, this);

      // set common moderators channel
      TopicModeratorsChannel = $"{Name}/moderators";

      Logger.LogDebug($"New topic '{Name}'");
    }

    /// <summary>
    /// Get first existing or new/unmoderated newRoom
    /// </summary>
    /// <param name="moderator">Moderator requesting newRoom</param>
    /// <returns>Room instance of topic</returns>
    public Room GetCreateRoom(Moderator moderator)
    {
      Room room = null;

      try
      {
        _rooms.Lock();

        // look if moderator was already assigned to newRoom
        room = Rooms.FirstOrDefault(x => x.Moderator != null && x.Moderator.UserId == moderator.UserId);
        if (room != null)
          Logger.LogDebug($"Returning existing moderated room '{room.Name}' by {moderator.UserId}");

        else
        {
          // look for unmoderated, existing room
          room = Rooms.FirstOrDefault(x => x.Moderator == null);
          if (room != null)
            Logger.LogDebug($"Returning existing unmoderated room '{room.Name}'");

          else
          { 
            var newRoom = new Room(this, _rooms.Count);
            int index = _rooms.Add(newRoom);

            Logger.LogDebug($"Created new room '{_rooms[index].Name}'");

            room = _rooms[index];
          }
        }

        return room;

      }
      finally
      {
        _rooms.Unlock();
      }

    }

    /// <summary>
    /// Get number of rooms in session
    /// </summary>
    /// <returns>Room count</returns>
    public int RoomCount()
    {

      try
      {
        _rooms.Lock();

        var count = _rooms.Count;
        return count;
      }
      finally
      {
        _rooms.Unlock();
      }

    }

    /// <summary>
    /// Get newRoom from a newRoom name
    /// </summary>
    /// <param name="roomName">Fully qualified newRoom name</param>
    /// <returns>Room, or null if not found</returns>
    public Room GetRoom(string roomName)
    {
      try
      {
        _rooms.Lock();

        var room = Rooms.FirstOrDefault(x => x.Name == roomName);
        if (room != null)
          Logger.LogDebug($"Found existing newRoom '{roomName}'");
        else
          Logger.LogError($"Room {roomName} does not exist");

        return room;

      }
      finally
      {
        _rooms.Unlock();
      }

    }

    /// <summary>
    /// Get session newRoom by index
    /// </summary>
    /// <param name="index"></param>
    /// <returns>Room</returns>
    public Room GetRoom(int index)
    {

      try
      {
        _rooms.Lock();

        if (index >= Rooms.Count)
          throw new ArgumentOutOfRangeException("Invalid topic newRoom instance argument");

        Room room = _rooms[index];
        return room;
      }
      finally
      {
        _rooms.Unlock();
      }

    }

    /// <summary>
    /// Gets newRoom for Participant
    /// </summary>
    /// <param name="participant">Participant to check</param>
    internal Room GetParticipantRoom(Participant participant)
    {
      try
      {
        _rooms.Lock();

        // go thru each newRoom and remove a (potential)
        // Participant
        foreach (Room room in Rooms)
        {
          if (room.ParticipantExists(participant))
            return room;
        }

        return null;
      }
      finally
      {
        _rooms.Unlock();
      }

    }

    /// <summary>
    /// Removes a Participant from the topic
    /// </summary>
    /// <param name="participant">Participant to remove</param>
    internal async Task RemoveParticipantAsync(Participant participant)
    {
      // first remove from atrium, if exists
      RemoveFromAtrium(participant);

      try
      {
        _rooms.Lock();

        Room emptyRoom = null;

        // go thru each newRoom and remove a (potential)
        // Participant
        foreach (Room room in Rooms)
        {
          await room.RemoveParticipantAsync(participant);

          // test if newRoom now has no moderator, meaning we can remove the newRoom
          //if (room.Moderator == null)
          //{
          //  Logger.LogDebug($"Room '{room.Name}' has it's moderator disconnected.  Deleting newRoom");
          //  emptyRoom = room;
          //}
        }

        // delete the newRoom (out of the enumeration)
        //Rooms.Remove(emptyRoom);

      }
      finally
      {
        _rooms.Unlock();
      }

      // finally remove (potential) moderator from moderators
      // command channel
      await Conference.RemoveConnectionToGroupAsync(
        TopicModeratorsChannel,
        participant.ConnectionId
      );
    }

    internal IList<Learner> AtriumGetContents()
    {
      try
      {
        atriumMutex.WaitOne();
        return _atrium.GetContents();
      }
      finally
      {
        atriumMutex.ReleaseMutex();
      }

    }

    /// <summary>
    /// Test if learner exists in atrium
    /// </summary>
    /// <param name="learner">Lerner to look for</param>
    /// <returns></returns>
    internal bool AtriumContains(Learner learner)
    {
      try
      {
        atriumMutex.WaitOne();
        return _atrium.Contains(learner);
      }
      finally
      {
        atriumMutex.ReleaseMutex();
      }
    }

    /// <summary>
    /// Remove connection id from atrium 
    /// </summary>
    /// <param name="connectionId">Connection remove</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    internal void RemoveFromAtrium(string connectionId)
    {
      try
      {
        atriumMutex.WaitOne();

        // try and remove connection.  if removed, notify all topic
        // moderators of atrium change
        if (_atrium.Remove(connectionId))
          Conference.SendMessage(
            new AtriumUpdateCommand(this, _atrium.GetContents()));

      }
      finally
      {
        atriumMutex.ReleaseMutex();
      }
    }

    /// <summary>
    /// Remove user from topic atrium 
    /// </summary>
    /// <param name="participant">Learner to remove</param>
    /// <returns>LEanrer found, or null</returns>
    /// <exception cref="NotImplementedException"></exception>
    internal Learner RemoveFromAtrium(Participant participant)
    {
      try
      {
        atriumMutex.WaitOne();

        // save so we can return the entire Participant
        // (which contains the contextId
        Learner learner = _atrium.Get(participant.UserId);

        // try and remove Participant.  if removed, notify all topic
        // moderators of atrium content change
        if (_atrium.Remove(participant))
          Conference.SendMessage(
            new AtriumUpdateCommand(this, _atrium.GetContents()));

        return learner;

      }
      finally
      {
        atriumMutex.ReleaseMutex();
      }
    }

    /// <summary>
    /// Add Participant to topic atrium
    /// </summary>
    /// <param name="participant">Leaner info</param>
    /// <param name="connectionId">Connection id</param>
    internal async Task AddToAtriumAsync(Learner participant)
    {
      try
      {
        atriumMutex.WaitOne();

        // add/replace Participant in atrium
        var learnerReplaced = _atrium.Upsert(participant);

        // if replaced a atrium contents, remove it from group
        if (learnerReplaced)
        {
          Logger.LogDebug($"Replacing existing '{Name}' atrium Participant '{participant.CommandChannel}'");
          await Conference.RemoveConnectionToGroupAsync(
            participant.CommandChannel,
            participant.ConnectionId);
        }

        // add Participant to its own group so it can receive newRoom assigments
        await Conference.AddConnectionToGroupAsync(participant);

        // notify Participant of atrium assignment
        Conference.SendMessage(
          new AtriumAssignmentCommand(participant, _atrium.Get(participant.UserId)));

        // notify all topic moderators of atrium change
        Conference.SendMessage(
          new AtriumUpdateCommand(this, _atrium.GetContents()));

      }
      finally
      {
        atriumMutex.ReleaseMutex();
      }

    }

    // removes a newRoom from the topic
    internal async Task RemoveRoomAsync(string roomId)
    {
      try
      {
        Logger.LogDebug($"Removing newRoom '{roomId}'");

        _rooms.Lock();

        Room room = Rooms.FirstOrDefault(x => x.Name == roomId);
        if (room == null)
          return;

        // remove the newRoom by removing the moderator
        // which deletes the newRoom
        await room.RemoveParticipantAsync(room.Moderator);

        // remove the newRoom from the topic
        _rooms.Remove(room.Index);

      }
      finally
      {
        _rooms.Unlock();
      }


    }

  }
}