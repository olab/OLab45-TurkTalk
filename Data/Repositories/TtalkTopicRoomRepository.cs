using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;

namespace OLab.TurkTalk.Data.Repositories;

public class TtalkTopicRoomRepository : GenericRepository<TtalkTopicRoom>
{
  public TtalkTopicRoomRepository(
    IOLabLogger logger,
    TTalkDBContext dbContext) : base(logger, dbContext)
  {
  }

  public async Task<TtalkTopicRoom> AssignModeratorAsync(
    uint roomId,
    uint moderatorId,
    bool commit = false)
  {
    var phys = DbContext.TtalkTopicRooms
      .FirstOrDefault(x => x.Id == roomId);
    phys.ModeratorId = moderatorId;

    DbContext.Update(phys);

    if ( commit )
      await DbContext.SaveChangesAsync();

    return phys;
  }

  public Task Create(uint topicId, TopicModerator moderator)
  {
    throw new NotImplementedException();
  }
}