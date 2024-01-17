﻿using Dawn;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.MessagePayloads;


namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class ConferenceTopicHelper
{
  public readonly IOLabLogger Logger;
  public IConference Conference;
  private readonly DatabaseUnitOfWork DbUnitOfWork;

  public ConferenceTopicHelper()
  {
  }

  public ConferenceTopicHelper(
    IOLabLogger logger,
    IConference conference,
    DatabaseUnitOfWork dbUnitOfWork) : this()
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(conference).NotNull(nameof(conference));
    Guard.Argument(dbUnitOfWork).NotNull(nameof(dbUnitOfWork));

    Logger = logger;

    Conference = conference;
    DbUnitOfWork = dbUnitOfWork;
  }

  //public TopicParticipant GetTopicParticipant(string sessionId)
  //{
  //  Guard.Argument(sessionId, nameof(sessionId)).NotEmpty();

  //  //var dto = Attendees.FirstOrDefault(x => x.SessionId == sessionId);
  //  //if (dto != null)
  //  //  return dto;
  //  return null;
  //}

  //public TopicParticipant GetModerator(string sessionId)
  //{
  //  Guard.Argument(sessionId, nameof(sessionId)).NotEmpty();

  //  //var dto = Attendees.FirstOrDefault(x => x.SessionId == sessionId);
  //  //if (dto != null)
  //  //  return dto;
  //  return null;
  //}


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
    Guard.Argument(dtoRequestLearner).NotNull(nameof(dtoRequestLearner));
    Guard.Argument(messageQueue).NotNull(nameof(messageQueue));

    //DatabaseUnitOfWork dbUnitOfWork = null;

    //try
    //{
    //  var mapper = new TopicParticipantMapper(Logger);

    //  dbUnitOfWork = new DatabaseUnitOfWork(Logger, Conference.TTDbContext);

    //  // see if already a known learner based on sessionId
    //  var dtoTopicParticipant = GetTopicParticipant(dtoRequestLearner.SessionId);

    //  // if not known to topic - create new learner and add to atrium
    //  if (dtoTopicParticipant == null)
    //  {
    //    var physLearner = mapper.DtoToPhysical(dtoRequestLearner);
    //    await dbUnitOfWork
    //      .TopicParticipantRepository
    //      .InsertAsync(physLearner);

    //    dtoRequestLearner = mapper.PhysicalToDto(physLearner);

    //    // add learner connection to learner-specific room channel
    //    messageQueue.EnqueueAddConnectionToGroupAction(
    //      dtoRequestLearner.ConnectionId,
    //      dtoRequestLearner.RoomLearnerSessionChannel);

    //    await Atrium.AddLearnerAsync(dtoRequestLearner, messageQueue);

    //    return;
    //  }

    //  // learner known, update participant with new connection id
    //  var physParticipant = dbUnitOfWork
    //    .TopicParticipantRepository
    //    .UpdateConnectionId(
    //      dtoRequestLearner.SessionId,
    //      dtoRequestLearner.ConnectionId);

    //  // test if was in room already 
    //  if (dtoTopicParticipant.RoomId != 0)
    //  {
    //    var dtoRoom = Rooms.FirstOrDefault(x => x.Id == dtoTopicParticipant.RoomId);

    //    // ensure room exists and has a moderator to receive them
    //    if (dtoRoom != null && dtoRoom.ModeratorId > 0)
    //    {
    //      Logger.LogInformation($"re-assigning learner '{dtoRequestLearner}' to room '{dtoRoom.Id}' with moderator {dtoRoom.ModeratorId}");

    //      // assign channel for room learners
    //      //messageQueue.EnqueueAddConnectionToGroupAction(
    //      //  dtoRequestLearner.ConnectionId,
    //      //  dtoRoom.RoomLearnersChannel);

    //      // signal attendee found in existing, moderated room
    //      messageQueue.EnqueueMessage(new RoomAcceptedMethod(
    //          Conference.Configuration,
    //          dtoRequestLearner.RoomLearnerSessionChannel,
    //          dtoRoom.Name,
    //          dtoRoom.Id,
    //          dtoTopicParticipant.SeatNumber,
    //          dtoRoom.Moderator.NickName,
    //          false));

    //      return;
    //    }

    //    // else, all other cases, add to atrium
    //    else
    //    {
    //      Logger.LogInformation($"learner '{dtoRequestLearner}' set for non-existant room.  asssigning to atrium");

    //      await Atrium.AddLearnerAsync(dtoRequestLearner, messageQueue);

    //      // change room to signify learner is in atrium
    //      physParticipant.RoomId = null;
    //      physParticipant.SeatNumber = null;
    //      dbUnitOfWork.TopicParticipantRepository.Update(physParticipant);

    //      return;
    //    }

    //  }
    //}
    //catch (Exception ex)
    //{
    //  Logger.LogError(ex, "AddLearnerAsync exception");
    //  throw;
    //}
    //finally
    //{
    //  dbUnitOfWork.Save();
    //}

  }

  /// <summary>
  /// Add moderator to topic
  /// </summary>
  /// <param name="moderatorDto">Moderator attendee</param>
  /// <param name="messageQueue">Resulting messages</param>
  /// <returns></returns>
  //internal async Task AddModeratorAsync(
  //  TopicParticipant moderatorDto,
  //  DispatchedMessages messageQueue)
  //{
  //  Guard.Argument(moderatorDto, nameof(moderatorDto)).NotNull();
  //  Guard.Argument(messageQueue, nameof(messageQueue)).NotNull();

  //  DatabaseUnitOfWork dbUnitOfWork = null;

  //  try
  //  {
  //    await SemaphoreLogger.WaitAsync(
  //      Logger,
  //      $"topic {Id}",
  //    _roomSemaphore);

  //    dbUnitOfWork = new DatabaseUnitOfWork(Logger, Conference.TTDbContext);

  //    // look if already a known moderator in loaded topic based on sessionId
  //    var dtoModerator = GetModerator(moderatorDto.SessionId);

  //    // create and add connection to topic moderators channel
  //    messageQueue.EnqueueAddConnectionToGroupAction(
  //      moderatorDto.ConnectionId,
  //      TopicModeratorsChannel);

  //    // new moderator. create new room and add moderator
  //    if (dtoModerator == null)
  //    {
  //      var newRoomDto = await CreateTopicRoomAsync(dbUnitOfWork);
  //      moderatorDto = await newRoomDto.AssignModeratorToRoom(
  //        dbUnitOfWork,
  //        moderatorDto,
  //        messageQueue);
  //    }

  //    // existing/known moderator
  //    else
  //    {
  //      var physRoom = dbUnitOfWork
  //        .TopicRoomRepository
  //        .Get(x => (x.Id == dtoModerator.RoomId) && (x.ModeratorId == dtoModerator.Id)).FirstOrDefault();

  //      var mapper = new TopicRoomMapper(Logger);
  //      var dtoRoom = mapper.PhysicalToDto(physRoom, this);

  //      // existing room exists for moderator, signal re-assign
  //      if (dtoRoom != null)
  //      {
  //        Logger.LogInformation($"re-assigned moderator {dtoModerator.Id} to topic '{Name}' room. id {dtoRoom.Id}");

  //        // TODO: signal attendees in room of moderator re-assigment
  //        return;
  //      }
  //      else
  //        Logger.LogError($"moderator id {dtoModerator.Id} room id {dtoModerator.RoomId} does not exist");
  //    }
  //  }
  //  catch (Exception ex)
  //  {
  //    Logger.LogError($"AddModeratorAsync error: {ex.Message}");
  //    throw;
  //  }
  //  finally
  //  {
  //    dbUnitOfWork.Save();

  //    SemaphoreLogger.Release(
  //      Logger,
  //      $"topic {Id}",
  //      _roomSemaphore);
  //  }
  //}

  /// <summary>
  /// Assign a learner to a room
  /// </summary>
  /// <param name="moderatorSessionId">moderator session id</param>
  /// <param name="learnerSessionId">learner session id</param>
  /// <param name="seatNumber">requested seat number</param>
  /// <param name="messageQueue">Dispatch messages</param>
  /// <returns></returns>
  internal void AssignLearnerToRoom(
    DispatchedMessages messageQueue,
    string moderatorSessionId,
    string learnerSessionId,
    uint? seatNumber = null)
  {
    Guard.Argument(messageQueue, nameof(messageQueue)).NotNull();
    Guard.Argument(moderatorSessionId, nameof(moderatorSessionId)).NotEmpty();
    Guard.Argument(learnerSessionId, nameof(learnerSessionId)).NotEmpty();

    var dbUnit = new DatabaseUnitOfWork(Logger, Conference.TTDbContext);

    //var physModerator = dbUnit
    //  .TopicParticipantRepository
    //  .GetBySessionId(moderatorSessionId);

    //// ensure learner exists
    //var physLearner = dbUnit
    //  .TopicParticipantRepository
    //  .GetBySessionId(learnerSessionId);

    //// auto assign seat number based on participants
    //// existing in the room
    //if (!seatNumber.HasValue)
    //  seatNumber = dbUnit
    //    .TopicRoomRepository
    //    .GetAvailableRoomSeat(physModerator.RoomId.Value);

    //if (seatNumber.HasValue)
    //{
    //  dbUnit.TopicParticipantRepository.AssignToRoom(
    //    learnerSessionId,
    //    physModerator.RoomId.Value,
    //    seatNumber);

    //  // signal room assignment to learner
    //  messageQueue.EnqueueMessage(new RoomAcceptedMethod(
    //      Conference.Configuration,
    //      $"{physLearner.TopicId}//{physModerator.RoomId}//{physLearner.SessionId}//session",
    //      physModerator.Room.Topic.Name,
    //      physModerator.Room.Id,
    //      seatNumber.Value,
    //      physModerator.UserName,
    //      false));

    //  // signal room assignment to moderator
    //  messageQueue.EnqueueMessage(new RoomAcceptedMethod(
    //      Conference.Configuration,
    //      $"{physModerator.TopicId}//{physModerator.RoomId}//moderator",
    //      physModerator.Room.Topic.Name,
    //      physModerator.Room.Id,
    //      seatNumber.Value,
    //      physModerator.UserName,
    //      false));
    //}

    dbUnit.Save();

  }

  internal async Task<TtalkConferenceTopic> GetCreateTopicAsync(
    IConference conference,
    string topicName)
  {
    Guard.Argument(conference, nameof(conference)).NotNull();
    Guard.Argument(topicName, nameof(topicName)).NotEmpty();

    try
    {
      await SemaphoreLogger.WaitAsync(
        Logger,
        $"topic '{topicName}' creation",
        Conference.TopicSemaphore);

      var phys =
        await DbUnitOfWork.ConferenceTopicRepository.GetCreateTopicAsync(conference.Id, topicName);

      return phys;

    }
    finally
    {
      SemaphoreLogger.Release(
        Logger,
        $"topic '{topicName}' creation",
        Conference.TopicSemaphore);
    }
  }

  internal async Task<TtalkConferenceTopic> GetAsync(uint topicId)
  {
    var phys = await DbUnitOfWork.ConferenceTopicRepository.GetByIdAsync(topicId);

    if (phys == null)
      Logger.LogWarning($"topic {topicId} does not exist");

    return phys;
  }

  internal async Task RegisterModeratorAsync(
    DispatchedMessages messageQueue,
    TtalkConferenceTopic physTopic,
    TtalkTopicParticipant physModerator)
  {
    Guard.Argument(messageQueue, nameof(messageQueue)).NotNull();
    Guard.Argument(physTopic, nameof(physTopic)).NotNull();
    Guard.Argument(physModerator, nameof(physModerator)).NotNull();

    // create and add connection to topic moderators channel
    messageQueue.EnqueueAddConnectionToGroupAction(
      physModerator.ConnectionId,
      physTopic.TopicModeratorsChannel);

    // send current atrium contents
    await SignalAtriumChangeAsync(
      physTopic,
      messageQueue);
  }

  internal async Task SignalAtriumChangeAsync(
    TtalkConferenceTopic physTopic,
    DispatchedMessages messageQueue)
  {
    Guard.Argument(physTopic, nameof(physTopic)).NotNull();
    Guard.Argument(messageQueue, nameof(messageQueue)).NotNull();

    try
    {
      await SemaphoreLogger.WaitAsync(
        Logger,
        $"topic {physTopic.Id} atrium",
        Conference.AtriumSemaphore);

      // load atrium users for topic
      var atriumLearners = DbUnitOfWork
        .TopicParticipantRepository
        .GetAtriumLearnersForTopic(physTopic.Id).OrderBy(x => x.NickName)
        .ToList();

      // signal to topic moderators atrium update
      messageQueue.EnqueueMessage(new AtriumUpdateMethod(
        Conference.Configuration,
        physTopic.TopicModeratorsChannel,
        atriumLearners));
    }
    finally
    {
      SemaphoreLogger.Release(
        Logger,
        $"topic {physTopic.Id} atrium",
        Conference.AtriumSemaphore);
    }

  }

  internal async Task AddLearnerToAtriumAsync(
    TtalkConferenceTopic physTopic,
    TtalkTopicParticipant physLearner,
    DispatchedMessages messageQueue)
  {
    Guard.Argument(physLearner, nameof(physLearner)).NotNull();
    Guard.Argument(messageQueue, nameof(messageQueue)).NotNull();
    Guard.Argument(physTopic, nameof(physTopic)).NotNull();

    Logger.LogInformation($"adding '{physLearner.NickName}' to atrium.");

    if (physLearner != null)
      // signal to learner 'new' add to atrium
      messageQueue.EnqueueMessage(new AtriumAcceptedMethod(
        Conference.Configuration,
        physLearner.RoomLearnerSessionChannel,
        physTopic.Name,
        true));

    await SignalAtriumChangeAsync(
      physTopic,
      messageQueue);
  }

  internal IList<TtalkTopicParticipant> GetTopicParticipants(uint topicId)
  {
    var physList = DbUnitOfWork
      .TopicParticipantRepository
      .GetParticipantsForTopic(topicId)
      .ToList();

    return physList;
  }

  /// <summary>
  /// Gets open/requested seat number
  /// </summary>
  /// <param name="atriumLearners">List of participants</param>
  /// <param name="seatNumberPerference">Seat number preference</param>
  /// <returns>New seat number</returns>
  internal uint GetSeatNumber(IList<TtalkTopicParticipant> atriumLearners, uint? seatNumberPerference)
  {
    uint seatNumber = 0;

    if (seatNumberPerference.HasValue)
    {
      seatNumber = seatNumberPerference.Value;
      Logger.LogInformation($"using seat number perference {seatNumber}");
    }

    else
    {
      for (uint i = 1; i <= 8; i++)
      {
        var seat = atriumLearners.FirstOrDefault(x => x.SeatNumber.HasValue && x.SeatNumber.Value == i);
        if (seat == null)
        {
          seatNumber = i;
          Logger.LogInformation($"found open seat number {seatNumber}");
          break;
        }
      }
    }

    return seatNumber;
  }
}
