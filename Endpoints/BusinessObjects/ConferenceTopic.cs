using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Mappers.Designer;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
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
  private SemaphoreSlim _roomSemaphore = new SemaphoreSlim(1, 1);

  public string TopicModeratorsChannel { get { return $"{Id}//moderators"; } }

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
    var dto = Attendees.FirstOrDefault(x => x.SessionId == sessionId);
    if (dto != null)
      return dto;
    return null;
  }

  public TopicModerator GetModerator(string sessionId)
  {
    var dto = Attendees.FirstOrDefault(x => x.SessionId == sessionId);
    if (dto != null)
      return new TopicModerator(dto);
    return null;
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
    try
    {
      _dbUnitOfWork = new DatabaseUnitOfWork(Logger, Conference.TTDbContext);

      // see if already a known learner based on sessionId
      var dtoParticipant = GetParticipant(dtoLearner.SessionId);

      // if not known previously - create new learner and add to topic atrium
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

        await _dbUnitOfWork.TopicParticipantRepository.InsertAsync(physAttendee);
        _dbUnitOfWork.Save();

        Logger.LogInformation($"assigned learner '{dtoLearner}' to {Name} atrium");

        await Atrium.AddLearnerAsync(dtoLearner, messageQueue);

        return;
      }

      // learner known, update participant with new connection id
      var physParticipant = _dbUnitOfWork.TopicParticipantRepository.UpdateConnectionId(
        dtoLearner.SessionId,
        dtoLearner.ConnectionId);

      // test if was in atrium (no room assigned)
      if (dtoParticipant.RoomId == 0)
      {
        Logger.LogInformation($"re-assigning learner '{dtoLearner}' to topic '{Name}' atrium");

        // signal 'resumption' of user in atrium
        messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
            Conference.Configuration,
            dtoLearner.RoomLearnerSessionChannel,
            this,
            false));

        return;
      }

      // found to be assigned to a room already
      else
      {
        var dtoRoom = Rooms.FirstOrDefault(x => x.Id == dtoParticipant.RoomId);

        // ensure room exists and has a moderator to receive them
        if (dtoRoom != null && dtoRoom.ModeratorId > 0)
        {
          Logger.LogInformation($"re-assigning learner '{dtoLearner}' to room '{dtoRoom.Id}' with moderator {dtoRoom.ModeratorId}");

          // signal attendee found in existing, moderated room
          messageQueue.EnqueueMessage(new RoomAcceptedMethod(
              Conference.Configuration,
              dtoLearner.RoomLearnerSessionChannel,
              dtoRoom,
              dtoParticipant.SeatNumber,
              false));

          return;
        }

        // else, all other cases, add to atrium
        else
        {
          Logger.LogInformation($"learner '{dtoLearner}' set for non-existant room.  asssigning to atrium");

          // no moderator, add to atrium
          messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
              Conference.Configuration,
              dtoLearner.RoomLearnerSessionChannel,
              this,
              true));

          // change room to signify learner is in atrium
          physParticipant.RoomId = null;
          physParticipant.SeatNumber = null;
          _dbUnitOfWork.TopicParticipantRepository.Update(physParticipant);

          return;
        }

      }
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "AddLearnerAsync exception");
      throw;
    }
    finally
    {
      _dbUnitOfWork.Save();
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
    DatabaseUnitOfWork dbUnitOfWork = null;

    try
    {
      await SemaphoreLogger.WaitAsync(
        Logger,
        $"topic {Id}",
      _roomSemaphore);

      dbUnitOfWork = new DatabaseUnitOfWork(Logger, Conference.TTDbContext);

      // look if already a known moderator based on sessionId
      var dtoModerator = GetModerator(moderator.SessionId);

      // create and add connection to topic moderators channel
      messageQueue.EnqueueAddToGroupAction(
        moderator.ConnectionId,
        TopicModeratorsChannel);

      // moderator not known previously. create new room and add moderator
      if (dtoModerator == null)
      {
        var physRoom = new TtalkTopicRoom
        {
          Name = Name,
          TopicId = Id
        };

        physRoom = await dbUnitOfWork
          .TopicRoomRepository
          .InsertAsync(physRoom);

        var newRoomDto = await TopicRoom.CreateRoomAsync(this, moderator);
        moderator.RoomId = newRoomDto.Id;

        // create and add connection to room moderator channel
        messageQueue.EnqueueAddToGroupAction(
          moderator.ConnectionId,
          newRoomDto.RoomModeratorChannel);

        // signal moderator added to new moderated room
        messageQueue.EnqueueMessage(new RoomAcceptedMethod(
            Conference.Configuration,
            newRoomDto.RoomModeratorChannel,
            newRoomDto,
            0,
            true));

      }
      else
      {
        // test if room still exists, and is moderated by attendee
        var existingRoomDto = Rooms.FirstOrDefault(x =>
          (x.Id == dtoModerator.RoomId) &&
          (x.ModeratorId == dtoModerator.Id));

        // existing room exists for moderator, signal re-assign
        if (existingRoomDto != null)
        {
          Logger.LogInformation($"re-assigned moderator {moderator.Id} to topic '{Name}' room. id {existingRoomDto.Id}");

          // signal moderator re-attaching to existing moderated room
          messageQueue.EnqueueMessage(new RoomAcceptedMethod(
              Conference.Configuration,
              existingRoomDto.RoomModeratorChannel,
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
          var physModerator =
            dbUnitOfWork.TopicParticipantRepository.AssignToRoom(existingRoomDto.Id, moderator.SessionId);

          // assign room to moderator
          var physRoom =
            dbUnitOfWork.TopicRoomRepository.AssignModeratorAsync(existingRoomDto.Id, moderator.Id);

          Logger.LogInformation($"assigned moderator {moderator.Id} to topic '{Name}' room. id {existingRoomDto.Id}");

          // signal moderator re-attaching to existing moderated room
          messageQueue.EnqueueMessage(new RoomAcceptedMethod(
              Conference.Configuration,
              existingRoomDto.RoomModeratorChannel,
              existingRoomDto,
              0,
              true));
        }
      }
    }
    catch (Exception ex)
    {
      Logger.LogError($"GetTopicAsync error: {ex.Message}");
      throw;
    }
    finally
    {
      dbUnitOfWork.Save();

      SemaphoreLogger.Release(
        Logger,
        $"topic {Id}",
        _roomSemaphore);
    }
  }

}
