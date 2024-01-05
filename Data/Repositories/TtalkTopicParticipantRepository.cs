﻿using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Spreadsheet;
using OLab.Common.Interfaces;
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
  /// Get a participant by session id
  /// </summary>
  /// <param name="sessionId">Session id</param>
  /// <returns>TtalkTopicParticipant </returns>
  /// <exception cref="KeyNotFoundException">session not found</exception>
  public TtalkTopicParticipant GetBySessionId(string sessionId)
  {
    // look for participant record for session and is a moderator
    // (with seat number is null)
    var phys = Get(
        filter: x => x.SessionId == sessionId, 
        includeProperties: "Room")
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
    var phys =
      Get(x => x.SessionId == sessionId)
        .FirstOrDefault();

    if (phys == null)
      throw new KeyNotFoundException($"unable to find participant for session id {sessionId} ");

    phys.ConnectionId = connectionId;
    Update(phys);

    return phys;
  }

  /// <summary>
  /// Get atrium learners for room
  /// </summary>
  /// <param name="roomId">Room id</param>
  /// <returns>List of learners</returns>
  public IList<TtalkTopicParticipant> GetAtriumLearnersForRoom(uint roomId)
  {
    var physList =
      Get(x => (x.RoomId == roomId) && ( !x.SeatNumber.HasValue )).ToList();

    return physList;
  }

  /// <summary>
  /// Get assigned learners for room
  /// </summary>
  /// <param name="roomId">Room id</param>
  /// <returns>List of learners</returns>
  public IList<TtalkTopicParticipant> GetLearnersForRoom(uint roomId)
  {
    var physList =
      Get(
        filter: x => (x.RoomId == roomId) && ( x.SeatNumber.HasValue && x.SeatNumber > 0 ),
        includeProperties: "Room").ToList();

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
    // look for participant record for session and is a moderator
    // (with seat number is null)
    var phys = Get(
      filter: x => (x.RoomId == roomId) && (x.SeatNumber == 0),
      includeProperties: "Room").FirstOrDefault();

    return phys;
  }
}
