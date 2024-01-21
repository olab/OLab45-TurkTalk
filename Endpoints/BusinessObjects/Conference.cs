using Dawn;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Models;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.Interface;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class QuestionSetting
{
  public string RoomName { get; set; }
}

public class Conference : IConference
{
  private IOLabConfiguration _configuration { get; }
  private OLabDBContext _dbContextOLab { get; }
  private TTalkDBContext _dbContextTtalk { get; }
  private ConferenceTopicHelper _topicHelper;

  public ConferenceTopicHelper TopicHelper 
  { 
    get { return _topicHelper; } 
  }

  private IOLabLogger _logger { get; }

  public uint Id { get; set; }
  public string Name { get; set; } = null!;

  public IOLabConfiguration Configuration { get { return _configuration; } }
  public IOLabLogger Logger { get { return _logger; } }
  public TTalkDBContext DbContextTtalk { get { return _dbContextTtalk; } }
  public OLabDBContext DbContextOLab { get { return _dbContextOLab; } }

  public Conference()
  {

  }

  public Conference(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContextOLab,
    TTalkDBContext dbContextTtalk)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContextOLab).NotNull(nameof(dbContextOLab));
    Guard.Argument(dbContextTtalk).NotNull(nameof(dbContextTtalk));

    _logger = OLabLogger.CreateNew<Conference>(loggerFactory);
    _configuration = configuration;
    _dbContextOLab = dbContextOLab;
    _dbContextTtalk = dbContextTtalk;

    // load the initial conference (for now, it's assumed to
    // only be one of, for now)
    var physConference = DbContextTtalk.TtalkConferences
      .FirstOrDefault() ?? throw new Exception("System conference record not defined");

    Id = physConference.Id;
    Name = physConference.Name;

    var dbUnitOfWork = new DatabaseUnitOfWork(
      _logger,
      _dbContextTtalk,
      _dbContextOLab);

    _topicHelper = new ConferenceTopicHelper(
      _logger,
      this,
      dbUnitOfWork);

    Logger.LogInformation($"conference'{Name}' ({Id}) loaded from conference");

  }

  /// <summary>
  /// Get/create new conference topic
  /// </summary>
  /// <param name="topicName">Topic to retrieve/create</param>
  /// <param name="createInDb">Optional flag to create in database, if not found</param>
  /// <returns>ConferenceTopic</returns>
  //public virtual async Task<ConferenceTopic> GetTopicAsync(
  //  TtalkTopicRoom physRoom,
  //  bool createInDb = true)
  //{
  //  Guard.Argument(physRoom).NotNull(nameof(physRoom));

  //  DatabaseUnitOfWork dbUnitOfWork = null;

  //  try
  //  {
  //    var mapper = new ConferenceTopicMapper(Logger);

  //    ConferenceTopic dtoTopic = null;

  //    await SemaphoreLogger.WaitAsync(
  //      Logger,
  //      $"room {physRoom.Name}",
  //      _topicSemaphore);

  //    dbUnitOfWork = new DatabaseUnitOfWork(_logger, TTDbContext);
  //    var physTopic = await dbUnitOfWork
  //      .ConferenceTopicRepository
  //      .GetByNameAsync(TTDbContext, physRoom.Name);


  //    // test if found topic in database
  //    if (physTopic != null)
  //    {
  //      Logger.LogInformation($"topic '{physTopic.Name}' exists in database");

  //      dbUnitOfWork.ConferenceTopicRepository.UpdateUsage(physTopic);

  //      dtoTopic = mapper.PhysicalToDto(physTopic, this);

  //      // update the atrium from the loaded topic attendees
  //      await dtoTopic.Atrium.LoadAsync(dtoTopic.Attendees);
  //    }


  //    // topic not found in database
  //    else if (createInDb)
  //    {
  //      physTopic = new TtalkConferenceTopic
  //      {
  //        Name = physRoom.Name,
  //        ConferenceId = Id,
  //        CreatedAt = DateTime.UtcNow,
  //      };

  //      await dbUnitOfWork.ConferenceTopicRepository.InsertAsync(physTopic);
  //      // explicit save needed because we need new inserted Id for mapper
  //      dbUnitOfWork.Save();
  //      Logger.LogInformation($"topic '{physTopic.Name}' ({physTopic.Id}) created in database");

  //      dtoTopic = mapper.PhysicalToDto(physTopic, this);

  //    }

  //    return dtoTopic;
  //  }
  //  catch (Exception ex)
  //  {
  //    Logger.LogError($"GetTopicAsync error: {ex.Message}");
  //    throw;
  //  }
  //  finally
  //  {
  //    dbUnitOfWork.Save();

  //    SemaphoreLogger.Release(
  //      Logger,
  //      $"room {physRoom.Name}",
  //      _topicSemaphore);
  //  }

  //}
}
