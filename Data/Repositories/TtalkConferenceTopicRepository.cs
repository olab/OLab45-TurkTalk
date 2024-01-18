using Dawn;
using Microsoft.EntityFrameworkCore;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;

namespace OLab.TurkTalk.Data.Repositories;

public partial class TtalkConferenceTopicRepository : GenericRepository<TtalkConferenceTopic>
{
  public TtalkConferenceTopicRepository(
    DatabaseUnitOfWork databaseUnitOfWork) : base(databaseUnitOfWork)
  {
  }

  public TtalkConferenceTopicRepository(
    IOLabLogger logger,
    TTalkDBContext dbContext) : base(logger, dbContext)
  {
  }

  public void UpdateUsage(TtalkConferenceTopic phys)
  {
    Guard.Argument(phys, nameof(phys)).NotNull();

    // update last used
    phys.LastusedAt = DateTime.UtcNow;
    Update(phys);

  }

  public async Task<TtalkConferenceTopic> GetByNameAsync(
    string roomName,
    uint nodeId )
  {
    Guard.Argument(roomName, nameof(roomName)).NotEmpty();
    Guard.Argument(nodeId, nameof(nodeId)).Positive();

    var physTopic = await DbContext
      .TtalkConferenceTopics
      .FirstOrDefaultAsync(x => x.Name == roomName && x.NodeId == nodeId);

    return physTopic;
  }

  public async Task<TtalkConferenceTopic> GetCreateTopicAsync(
    uint conferenceId,
    uint nodeId,
    string topicName)
  {
    Guard.Argument(conferenceId, nameof(conferenceId)).Positive();
    Guard.Argument(nodeId, nameof(nodeId)).Positive();
    Guard.Argument(topicName, nameof(topicName)).NotEmpty();

    var phys = await GetByNameAsync(topicName, nodeId);
    if (phys == null)
    {
      phys = new TtalkConferenceTopic
      {
        ConferenceId = conferenceId,
        Name = topicName,
        NodeId = nodeId,
        CreatedAt = DateTime.UtcNow
      };

      await InsertAsync(phys);

      // explicit save needed because we need new inserted Id 
      DbUnitOfWork.Save();

      Logger.LogInformation($"created topic {phys.Name} ({phys.Id})");
    }

    return phys;
  }
}
