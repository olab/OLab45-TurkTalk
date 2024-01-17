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
    string roomName)
  {
    Guard.Argument(roomName, nameof(roomName)).NotEmpty();

    var physTopic = await DbContext
      .TtalkConferenceTopics
      .Include(x => x.TtalkTopicRooms)
      .ThenInclude(x => x.TtalkTopicParticipants)
      .FirstOrDefaultAsync(x => x.Name == roomName);

    return physTopic;
  }

  public async Task<TtalkConferenceTopic> GetCreateTopicAsync(
    uint conferenceId,
    string topicName)
  {
    Guard.Argument(conferenceId, nameof(conferenceId)).Positive();
    Guard.Argument(topicName, nameof(topicName)).NotEmpty();

    var phys = await GetByNameAsync(topicName);
    if (phys == null)
    {
      phys = new TtalkConferenceTopic
      {
        ConferenceId = conferenceId,
        Name = topicName,
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
