using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class ConferenceTopic
{
  public uint Id { get; set; }
  public string Name { get; internal set; }
  public uint ConferenceId { get; internal set; }
  public DateTime CreatedAt { get; set; }
  public DateTime LastUsedAt { get; set; }

  public TopicAtrium Atrium;
  public Conference Conference;
  public IList<TopicParticipant> Attendees { get; set; }
  public IList<TopicRoom> Rooms { get; set; }

  public IOLabLogger Logger { get { return Conference.Logger; } }

  public ConferenceTopic()
  {
    CreatedAt = DateTime.UtcNow;
    LastUsedAt = DateTime.UtcNow;
    Atrium = new TopicAtrium(this);
    Attendees = new List<TopicParticipant>();
    Rooms = new List<TopicRoom>();
  }

  public ConferenceTopic(Conference conference) : this()
  {
    Conference = conference;
  }

  public TopicParticipant GetParticipant(string sessionId)
  {
    var dtoAttendee = Attendees.FirstOrDefault(x => x.SessionId == sessionId);
    return dtoAttendee;
  }

  /// <summary>
  /// Add a learner to a topic
  /// </summary>
  /// <param name="dtoLearner">Learner to add</param>
  /// <param name="messageQueue">Resulting messages</param>
  /// <returns></returns>
  public async Task AddLearnerAsync(
    TopicLearner dtoLearner,
    DispatchedMessages messageQueue)
  {
    // look if already a known attendee based on sessionId
    var dtoParticipant = GetParticipant(dtoLearner.SessionId);

    // test if not known previously - create new learner and add to atrium
    if (dtoParticipant == null)
    {
      var physAttendee = new TtalkTopicParticipant
      {
        SessionId = dtoLearner.SessionId,
        TopicId = Id,
        UserId = dtoLearner.UserId,
        UserName = dtoLearner.UserName,
        TokenIssuer = dtoLearner.TokenIssuer,
        ConnectionId = dtoLearner.ConnectionId
        // not setting a roomId implies learner is in atrium
      };

      await Conference.TTDbContext.TtalkTopicParticipants.AddAsync(physAttendee);
      await Conference.TTDbContext.SaveChangesAsync();

      Logger.LogInformation($"assigned learner '{dtoLearner}' to {Name} atrium");

      // signal 'new' add to atrium
      messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
          Conference.Configuration,
          dtoLearner.ChatChannel,
          this,
          true));

      return;
    }

    // update participant with new connection id
    var physParticipant = 
      Conference.TTDbContext.TtalkTopicParticipants.FirstOrDefault(( x => x.SessionId ==  dtoLearner.SessionId ));
    physParticipant.ConnectionId = dtoLearner.ConnectionId;
    Conference.TTDbContext.TtalkTopicParticipants.Update( physParticipant );

    // known participant. test if was in atrium (no room assigned)
    if (dtoParticipant.RoomId == 0)
    {
      Logger.LogInformation($"re-assigning learner '{dtoLearner}' to topic '{Name}' atrium");

      // signal 'resumption' of user in atrium
      messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
          Conference.Configuration,
          dtoLearner.ChatChannel,
          this,
          false));

      return;
    }

    // known participant. found to be assigned to a room already
    else
    {
      var dtoRoom = Rooms.FirstOrDefault(x => x.Id == dtoParticipant.RoomId);

      // ensure room exists and has a moderator to receive them
      if (dtoRoom != null && dtoRoom.ModeratorId > 0 )
      {
        Logger.LogInformation($"re-assigning learner '{dtoLearner}' to room '{dtoRoom.Id}' with moderator {dtoRoom.ModeratorId}");

        // signal attendee found in existing, moderated room
        messageQueue.EnqueueMessage(new RoomAcceptedMethod(
            Conference.Configuration,
            dtoLearner.ChatChannel,
            dtoRoom,
            dtoParticipant.SeatNumber,
            false));

        return;
      }

      // else, all other cases, add to atrium
      else
      {
        // no moderator, add to atrium
        messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
            Conference.Configuration,
            dtoLearner.ChatChannel,
            this,
            true));

        return;
      }
    }
  }

  /// <summary>
  /// Add moderator to topic
  /// </summary>
  /// <param name="moderator">Moderator attendee</param>
  /// <param name="messageQueue">Resulting messages</param>
  /// <returns></returns>
  internal async Task AddModeratorAsync(
    TopicModerator moderator,
    DispatchedMessages messageQueue)
  {
    // look if already a known attendee based on sessionId
    var dtoModerator = GetParticipant(moderator.SessionId) as TopicModerator;

    // moderator not attendee previously. create moderator and add to a new room
    if (dtoModerator == null)
    {
      var newRoomDto = await TopicRoom.CreateRoomAsync(this, moderator);

      // signal moderator added to new, moderated room
      messageQueue.EnqueueMessage(new RoomAcceptedMethod(
          Conference.Configuration,
          dtoModerator.RoomChannel,
          newRoomDto,
          0,
          true));

      return;

    }

    // test if room still exists, and is moderated by attendee
    var existingRoomDto = Rooms.FirstOrDefault(x =>
      (x.Id == moderator.RoomId) &&
      (x.ModeratorId == dtoModerator.Id));

    // existing room exists for moderator, signal re-assign
    if (existingRoomDto != null)
    {
      Logger.LogInformation($"re-assigned moderator {moderator.Id} to topic '{Name}' room. id {existingRoomDto.Id}");

      // signal moderator re-attaching to existing moderated room
      messageQueue.EnqueueMessage(new RoomAcceptedMethod(
          Conference.Configuration,
          moderator.RoomChannel,
          existingRoomDto,
          0,
          false));

      // TODO: signal attendees in room of moderator re-assigment
      return;
    }

    // room did not exist, so create and assign moderator to it
    else
    {
      existingRoomDto = await TopicRoom.CreateRoomAsync(this, moderator);

      // assign moderator to room
      var physModerator = Conference.TTDbContext.TtalkTopicParticipants
        .FirstOrDefault(x => x.SessionId == moderator.SessionId);
      physModerator.RoomId = existingRoomDto.Id;

      Conference.TTDbContext.TtalkTopicParticipants.Update(physModerator);
      await Conference.TTDbContext.SaveChangesAsync();

      // assign room to moderator
      var physRoom = Conference.TTDbContext.TtalkTopicRooms
        .FirstOrDefault(x => x.Id == physModerator.Id);
      Conference.TTDbContext.TtalkTopicRooms.Update(physRoom);
      await Conference.TTDbContext.SaveChangesAsync();

      Logger.LogInformation($"assigned moderator {moderator.Id} to topic '{Name}' room. id {existingRoomDto.Id}");

      // signal moderator re-attaching to existing moderated room
      messageQueue.EnqueueMessage(new RoomAcceptedMethod(
          Conference.Configuration,
          moderator.RoomChannel,
          existingRoomDto,
          0,
          true));
    }
  }

}
