using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.BusinessObjects;

namespace OLab.TurkTalk.Data.Repositories;

public class TtalkTopicRoomRepository : GenericRepository<TtalkTopicRoom>
{
  public TtalkTopicRoomRepository(
    DatabaseUnitOfWork databaseUnitOfWork) : base(databaseUnitOfWork)
  {
  }

  public TtalkTopicRoomRepository(
    IOLabLogger logger,
    TTalkDBContext dbContext) : base(logger, dbContext)
  {
  }

  /// <summary>
  /// Gets the room associated with a moderator session id
  /// </summary>
  /// <param name="moderatorSessionId">moderator session id</param>
  /// <returns>Topic room</returns>
  public TtalkTopicRoom GetModeratorRoom(string moderatorSessionId)
  {
    TtalkTopicRoom physRoom = null;

    // look for participant record for session and is a moderator
    // (with seat number is null)
    var phys = DbUnitOfWork
      .TopicParticipantRepository
      .Get(x => (x.SessionId == moderatorSessionId) && (!x.SeatNumber.HasValue)).FirstOrDefault();

    if (phys != null)
      physRoom = DbUnitOfWork
        .TopicRoomRepository
        .Get(x => x.Id == phys.RoomId).FirstOrDefault();

    if (physRoom == null)
      DbUnitOfWork.Logger.LogError($"unable to find room for moderator session {moderatorSessionId} ");

    return physRoom;
  }

  /// <summary>
  /// Assigns a moderator to a room
  /// </summary>
  /// <param name="roomId">room id</param>
  /// <param name="moderatorId">moderator id</param>
  /// <param name="commit">optional commit to database</param>
  /// <returns></returns>
  public async Task<TtalkTopicRoom> AssignModeratorAsync(
    uint roomId,
    uint moderatorId,
    bool commit = false)
  {
    var phys = DbContext.TtalkTopicRooms
      .FirstOrDefault(x => x.Id == roomId);
    phys.ModeratorId = moderatorId;

    DbContext.Update(phys);

    if (commit)
      await DbContext.SaveChangesAsync();

    return phys;
  }

  /// <summary>
  /// GEt available room seat from room
  /// </summary>
  /// <param name="roomId">Room id</param>
  /// <returns>Seat number (>0)</returns>
  public uint? GetAvailableRoomSeat(uint roomId)
  {
    var seatList = DbUnitOfWork
      .TopicParticipantRepository
      .GetLearnersForRoom(roomId).Select(x => x.SeatNumber).ToList();

    for (uint i = 0; i < 8; i++)
    {
      if (!seatList.Contains(i))
      {
        DbUnitOfWork.Logger.LogInformation($"found seat {i} available for room id {roomId}");
        return i;
      }
    }

    DbUnitOfWork.Logger.LogError($"found no seat available for room id {roomId}");
    return null;

  }
}