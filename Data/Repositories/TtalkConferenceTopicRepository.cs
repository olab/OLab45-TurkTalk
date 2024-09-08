using Dawn;
using Microsoft.EntityFrameworkCore;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.BusinessObjects;

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

  /// <summary>
  /// Retreive conference topic by name
  /// </summary>
  /// <param name="topicName">Topic Name</param>
  /// <param name="mapId">Map Id</param>
  /// <returns>TtalkConferenceTopic</returns>
  public async Task<TtalkConferenceTopic> GetByNameAsync(
    string topicName,
    uint mapId )
  {
    Guard.Argument(topicName, nameof(topicName)).NotEmpty();
    Guard.Argument(mapId, nameof(mapId)).Positive();

    var physTopic = await DbContext
      .TtalkConferenceTopics
      .Include("TtalkTopicParticipants")
      .FirstOrDefaultAsync(x => x.Name == topicName && x.MapId == mapId);

    return physTopic;
  }

  /// <summary>
  /// Creates a conference topic record
  /// </summary>
  /// <param name="conferenceId">Conference Id</param>
  /// <param name="mapId">Map Id</param>
  /// <param name="topicName">topic Name</param>
  /// <returns>TtalkConferenceTopic</returns>
  public async Task<TtalkConferenceTopic> GetCreateTopicAsync(
    uint conferenceId,
    uint mapId,
    string topicName)
  {
    Guard.Argument(conferenceId, nameof(conferenceId)).Positive();
    Guard.Argument(mapId, nameof(mapId)).Positive();
    Guard.Argument(topicName, nameof(topicName)).NotEmpty();

    var physTopic = await GetByNameAsync(topicName, mapId);
    if (physTopic == null)
    {
      physTopic = new TtalkConferenceTopic
      {
        ConferenceId = conferenceId,
        Name = topicName,
        MapId = mapId,
        CreatedAt = DateTime.UtcNow
      };

      await InsertAsync(physTopic);

      // explicit save needed because we need new inserted Id 
      DbUnitOfWork.Save();

      Logger.LogInformation($"created topic {physTopic.Name} ({physTopic.Id})");
    }

    return physTopic;
  }
}
