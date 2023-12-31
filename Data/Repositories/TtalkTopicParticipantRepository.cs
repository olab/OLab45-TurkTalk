using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Repositories;

namespace OLab.TurkTalk.Data.Models;

public partial class TtalkTopicParticipantRepository : GenericRepository<TtalkTopicParticipant>
{
  public TtalkTopicParticipantRepository(
    IOLabLogger logger,
    TTalkDBContext dbContext) : base(logger, dbContext)
  {
  }

  /// <summary>
  /// Assign participant to room
  /// </summary>
  /// <param name="roomId">Topic room id</param>
  /// <param name="sessionId">OLAb session id</param>
  /// <returns>Changed record</returns>
  public TtalkTopicParticipant AssignToRoom(uint roomId, string sessionId)
  {
    var phys = DbContext.TtalkTopicParticipants
      .FirstOrDefault(x => x.SessionId == sessionId);
    phys.RoomId = roomId;

    DbContext.TtalkTopicParticipants.Update(phys);
    return phys;
  }

  /// <summary>
  /// Update a participant's connectionId
  /// </summary>
  /// <param name="sessionId">OLab session id</param>
  /// <param name="connectionId">new connection id</param>
  /// <returns>Changed record</returns>
  public TtalkTopicParticipant UpdateConnectionId(
    string sessionId,
    string connectionId)
  {
    var phys =
      DbContext.TtalkTopicParticipants.FirstOrDefault((x => x.SessionId == sessionId));

    if (phys == null)
    {
      phys.ConnectionId = connectionId;
      DbContext.TtalkTopicParticipants.Update(phys);
    }

    return phys;
  }
}
