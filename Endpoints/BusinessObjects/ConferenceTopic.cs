using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Mappers.Designer;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.Mappers;
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

  public TopicParticipant GetModerator(string sessionId)
  {
    var dto = Attendees.FirstOrDefault(x => x.SessionId == sessionId);
    if (dto != null)
      return dto;
    return null;
  }


  /// <summary>
  /// Add a learner to a topic
  /// </summary>
  /// <param name="dtoLearner">Learner to add</param>
  /// <param name="messageQueue">Resulting messages</param>
  /// <returns></returns>
  public async Task AddLearnerAsync(
    TopicParticipant dtoLearner,
    DispatchedMessages messageQueue)
  {
    DatabaseUnitOfWork dbUnitOfWork = null;

    try
    {
      messageQueue.EnqueueAddConnectionToGroupAction(
        dtoLearner.ConnectionId,
        dtoLearner.RoomLearnerSessionChannel);

      dbUnitOfWork = new DatabaseUnitOfWork(Logger, Conference.TTDbContext);

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

        await dbUnitOfWork.TopicParticipantRepository.InsertAsync(physAttendee);
        dbUnitOfWork.Save();

        Logger.LogInformation($"assigned learner '{dtoLearner}' to {Name} atrium");

        await Atrium.AddLearnerAsync(dtoLearner, messageQueue);

        return;
      }

      // learner known, update participant with new connection id
      var physParticipant = dbUnitOfWork.TopicParticipantRepository.UpdateConnectionId(
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

          // assign channel for room learners
          messageQueue.EnqueueAddConnectionToGroupAction(
            dtoLearner.ConnectionId,
            dtoRoom.RoomLearnersChannel);

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
          dbUnitOfWork.TopicParticipantRepository.Update(physParticipant);

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
      dbUnitOfWork.Save();
    }

  }

  private async Task<TopicRoom> CreateTopicRoomAsync(DatabaseUnitOfWork dbUnitOfWork)
  {
    var physRoom = new TtalkTopicRoom
    {
      Name = Name,
      TopicId = Id
    };

    physRoom = await dbUnitOfWork
      .TopicRoomRepository
      .InsertAsync(physRoom);
    dbUnitOfWork.Save();

    var mapper = new TopicRoomMapper(Logger);
    var roomDto = mapper.PhysicalToDto(physRoom, this);

    return roomDto;

  }

  /// <summary>
  /// Add moderator to topic
  /// </summary>
  /// <param name="moderatorDto">Moderator attendee</param>
  /// <param name="messageQueue">Resulting messages</param>
  /// <returns></returns>
  internal async Task AddModeratorAsync(
    TopicParticipant moderatorDto,
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

      // look if already a known moderator in loaded topic based on sessionId
      var dtoModerator = GetModerator(moderatorDto.SessionId);

      // create and add connection to topic moderators channel
      messageQueue.EnqueueAddConnectionToGroupAction(
        moderatorDto.ConnectionId,
        TopicModeratorsChannel);

      // new moderator. create new room and add moderator
      if (dtoModerator == null)
      {
        var newRoomDto = await CreateTopicRoomAsync(dbUnitOfWork);
        moderatorDto = await newRoomDto.AssignModerator(
          dbUnitOfWork,
          moderatorDto,
          messageQueue);
      }

      // existing/known moderator
      else
      {
        var physRoom = dbUnitOfWork
          .TopicRoomRepository
          .Get(x => (x.Id == dtoModerator.RoomId) && (x.ModeratorId == dtoModerator.Id)).FirstOrDefault();

        var mapper = new TopicRoomMapper(Logger);
        var dtoRoom = mapper.PhysicalToDto(physRoom, this);

        // existing room exists for moderator, signal re-assign
        if (dtoRoom != null)
        {
          Logger.LogInformation($"re-assigned moderator {dtoModerator.Id} to topic '{Name}' room. id {dtoRoom.Id}");

          // TODO: signal attendees in room of moderator re-assigment
          return;
        }
        else
          Logger.LogError($"moderator id {dtoModerator.Id} room id {dtoModerator.RoomId} does not exist");
      }
    }
    catch (Exception ex)
    {
      Logger.LogError($"AddModeratorAsync error: {ex.Message}");
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
