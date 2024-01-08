﻿using Dawn;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Spreadsheet;
using OLab.Common.Interfaces;
using OLab.Data.Models;
using OLab.TurkTalk.Data.Repositories;

namespace OLab.TurkTalk.Data.Models;

public partial class TtalkTopicParticipantRepository : GenericRepository<TtalkTopicParticipant>
{
  public TtalkTopicParticipantRepository(
    DatabaseUnitOfWork databaseUnitOfWork) : base(databaseUnitOfWork)
  {
  }

  public TtalkTopicParticipantRepository(
    IOLabLogger logger,
    TTalkDBContext dbContext) : base(logger, dbContext)
  {
  }

  /// <summary>
  /// Get a moderator by session id
  /// </summary>
  /// <param name="connectionId">Session id</param>
  /// <returns>TtalkTopicParticipant </returns>
  /// <exception cref="KeyNotFoundException">session not found</exception>
  public TtalkTopicParticipant GetLearnerBySessionId(string sessionId)
  {
    Guard.Argument(sessionId, nameof(sessionId)).NotEmpty();

    var phys = Get(
        filter: x => (x.SessionId == sessionId) &&
          ((x.SeatNumber.HasValue && x.SeatNumber > 0) || (!x.SeatNumber.HasValue)),
        includeProperties: "Room, Room.Topic")
      .FirstOrDefault();

    return phys;
  }

  /// <summary>
  /// Get a moderator by session id
  /// </summary>
  /// <param name="connectionId">Session id</param>
  /// <returns>TtalkTopicParticipant </returns>
  /// <exception cref="KeyNotFoundException">session not found</exception>
  public TtalkTopicParticipant GetModeratorBySessionId(string sessionId)
  {
    Guard.Argument(sessionId, nameof(sessionId)).NotEmpty();

    var phys = Get(
        filter: x => (x.SessionId == sessionId) && (x.SeatNumber == 0),
        includeProperties: "Room, Room.Topic")
      .FirstOrDefault();

    return phys;
  }

  /// <summary>
  /// Get a participant by connection id
  /// </summary>
  /// <param name="connectionId">Session id</param>
  /// <returns>TtalkTopicParticipant </returns>
  /// <exception cref="KeyNotFoundException">session not found</exception>
  public TtalkTopicParticipant GetByConnectionId(string connectionId)
  {
    Guard.Argument(connectionId, nameof(connectionId)).NotEmpty();

    var phys = Get(
        filter: x => x.ConnectionId == connectionId,
        includeProperties: "Room, Topic")
      .FirstOrDefault();

    if (phys == null)
      throw new KeyNotFoundException($"unable to find participant with session {connectionId} ");

    return phys;
  }

  /// <summary>
  /// Get a participant by session id
  /// </summary>
  /// <param name="sessionId">Session id</param>
  /// <returns>TtalkTopicParticipant </returns>
  /// <exception cref="KeyNotFoundException">session not found</exception>
  public TtalkTopicParticipant GetBySessionId(string sessionId)
  {
    Guard.Argument(sessionId, nameof(sessionId)).NotEmpty();

    // look for participant record for session and is a moderator
    // (with seat number is null)
    var phys = Get(
        filter: x => x.SessionId == sessionId,
        includeProperties: "Room, Topic")
      .FirstOrDefault();

    if (phys == null)
      throw new KeyNotFoundException($"unable to find participant with session {sessionId} ");

    return phys;
  }

  /// <summary>
  /// Assign participant to room
  /// </summary>
  /// <param name="sessionId">OLab session id</param>
  /// <param name="roomId">Topic room id</param>
  /// <param name="seatNumber">Seat number</param>
  /// <returns>Changed record</returns>
  /// <exception cref="KeyNotFoundException">session id not found</exception> 
  public TtalkTopicParticipant AssignToRoom(
    string sessionId,
    uint roomId,
    uint? seatNumber)
  {
    Guard.Argument(sessionId, nameof(sessionId)).NotEmpty();
    Guard.Argument(roomId, nameof(roomId)).Positive();

    var phys = Get(x => x.SessionId == sessionId)
      .FirstOrDefault();

    if (phys == null)
      throw new KeyNotFoundException($"unable to find participant for session id {sessionId} ");

    phys.RoomId = roomId;
    phys.SeatNumber = seatNumber;

    Logger.LogInformation($"assigning session {sessionId} to room id {roomId}, seat {seatNumber}");

    DbContext
      .TtalkTopicParticipants
      .Update(phys);

    return phys;
  }

  /// <summary>
  /// Update a participant's connectionId
  /// </summary>
  /// <param name="sessionId">OLab session id</param>
  /// <param name="connectionId">new connection id</param>
  /// <returns>Changed record</returns>
  /// <exception cref="KeyNotFoundException">session id not found</exception> 
  public TtalkTopicParticipant UpdateConnectionId(
    string sessionId,
    string connectionId)
  {
    Guard.Argument(sessionId, nameof(sessionId)).NotEmpty();
    Guard.Argument(connectionId, nameof(connectionId)).NotEmpty();

    var phys =
      Get(x => x.SessionId == sessionId)
        .FirstOrDefault();

    if (phys == null)
      throw new KeyNotFoundException($"unable to find participant for session id '{sessionId}' ");

    var oldConnectionId = phys.ConnectionId;
    phys.ConnectionId = connectionId;

    Update(phys);

    Logger.LogInformation($"updated participant session '{sessionId}' '{oldConnectionId}' -> '{connectionId}'");
    return phys;
  }

  /// <summary>
  /// Get atrium learners for topic
  /// </summary>
  /// <param name="topicId">Topic id</param>
  /// <returns>List of learners</returns>
  public IList<TtalkTopicParticipant> GetAtriumLearnersForTopic(uint topicId)
  {
    Guard.Argument(topicId, nameof(topicId)).Positive();

    var physList =
      Get(x => (x.TopicId == topicId) && (!x.SeatNumber.HasValue)).ToList();

    Logger.LogInformation($"found '{physList.Count}' atrium learners for topic id '{topicId}'");

    return physList;
  }

  /// <summary>
  /// Get assigned learners for room
  /// </summary>
  /// <param name="roomId">Room id</param>
  /// <returns>List of learners</returns>
  public IList<TtalkTopicParticipant> GetLearnersForRoom(uint roomId)
  {
    Guard.Argument(roomId, nameof(roomId)).Positive();

    var physList =
      Get(
        filter: x => (x.RoomId == roomId) && (x.SeatNumber.HasValue && x.SeatNumber > 0),
        includeProperties: "Room, Room.Topic").ToList();

    return physList;
  }

  /// <summary>
  /// Get all participants for room
  /// </summary>
  /// <param name="roomId">Room id</param>
  /// <returns>List of learners</returns>
  public IList<TtalkTopicParticipant> GetParticipantsForRoom(uint roomId)
  {
    Guard.Argument(roomId, nameof(roomId)).Positive();

    var physList =
      Get(
        filter: x => (x.RoomId == roomId),
        includeProperties: "Room, Room.Topic").ToList();

    return physList;
  }

  /// <summary>
  /// Gets the moderator associated with a room 
  /// </summary>
  /// <param name="roomId">room id</param>
  /// <returns>Room moderator</returns>
  /// <exception cref="KeyNotFoundException">room not found</exception> 
  public TtalkTopicParticipant GetModeratorforRoom(uint roomId)
  {
    Guard.Argument(roomId, nameof(roomId)).Positive();

    // look for participant record for session and is a moderator
    // (with seat number is null)
    var phys = Get(
      filter: x => (x.RoomId == roomId) && (x.SeatNumber == 0),
      includeProperties: "Room").FirstOrDefault();

    return phys;
  }

  /// <summary>
  /// Get participants for topic
  /// </summary>
  /// <param name="topicId">Topic id</param>
  /// <returns>List of TTalkParticipant</returns>
  public List<TtalkTopicParticipant> GetParticipantsForTopic(uint topicId)
  {
    Guard.Argument(topicId, nameof(topicId)).Positive();

    var physList = Get(
      filter: x => x.TopicId == topicId,
      includeProperties: "Room,Topic")
      .ToList();

    return physList;
  }
}
