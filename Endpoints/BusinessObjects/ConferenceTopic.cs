using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Mappers.Designer;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.Interface;
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

  public ITopicAtrium Atrium;
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
    Atrium = new TopicAtrium(
      Name, 
      Logger, 
      Conference.Configuration);
    Attendees = new List<TopicParticipant>();
    Rooms = new List<TopicRoom>();
  }

  public ConferenceTopic(Conference conference) : this()
  {
    Conference = conference;
  }

  public TopicParticipant GetTopicParticipant(string sessionId)
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
  /// <param name="dtoRequestLearner">Learner to add</param>
  /// <param name="messageQueue">Resulting messages</param>
  /// <returns></returns>
  public async Task AddLearnerAsync(
    TopicParticipant dtoRequestLearner,
    DispatchedMessages messageQueue)
  {
    DatabaseUnitOfWork dbUnitOfWork = null;

    try
    {
      var mapper = new TopicParticipantMapper(Logger);

      dbUnitOfWork = new DatabaseUnitOfWork(Logger, Conference.TTDbContext);

      // see if already a known learner based on sessionId
      var dtoTopicParticipant = GetTopicParticipant(dtoRequestLearner.SessionId);

      // if not known to topic - create new learner and add to atrium
      if (dtoTopicParticipant == null)
      {
        var physLearner = mapper.DtoToPhysical(dtoRequestLearner);
        await dbUnitOfWork
          .TopicParticipantRepository
          .InsertAsync(physLearner);

        dtoRequestLearner = mapper.PhysicalToDto(physLearner);

        // add learner connection to learner-specific room channel
        messageQueue.EnqueueAddConnectionToGroupAction(
          dtoRequestLearner.ConnectionId,
          dtoRequestLearner.RoomLearnerSessionChannel);

        Atrium.AddLearner(dtoRequestLearner, messageQueue);

        return;
      }

      // learner known, update participant with new connection id
      var physParticipant = dbUnitOfWork
        .TopicParticipantRepository
        .UpdateConnectionId(
          dtoRequestLearner.SessionId,
          dtoRequestLearner.ConnectionId);

      // test if was in room already 
      if (dtoTopicParticipant.RoomId != 0)
      {
        var dtoRoom = Rooms.FirstOrDefault(x => x.Id == dtoTopicParticipant.RoomId);

        // ensure room exists and has a moderator to receive them
        if (dtoRoom != null && dtoRoom.ModeratorId > 0)
        {
          Logger.LogInformation($"re-assigning learner '{dtoRequestLearner}' to room '{dtoRoom.Id}' with moderator {dtoRoom.ModeratorId}");

          // assign channel for room learners
          messageQueue.EnqueueAddConnectionToGroupAction(
            dtoRequestLearner.ConnectionId,
            dtoRoom.RoomLearnersChannel);

          // signal attendee found in existing, moderated room
          messageQueue.EnqueueMessage(new RoomAcceptedMethod(
              Conference.Configuration,
              dtoRequestLearner.RoomLearnerSessionChannel,
              dtoRoom,
              dtoTopicParticipant.SeatNumber,
              false));

          return;
        }

        // else, all other cases, add to atrium
        else
        {
          Logger.LogInformation($"learner '{dtoRequestLearner}' set for non-existant room.  asssigning to atrium");

          Atrium.AddLearner(dtoRequestLearner, messageQueue);

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
