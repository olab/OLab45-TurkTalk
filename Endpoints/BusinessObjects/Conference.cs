using Dawn;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Models;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.Interface;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class QuestionSetting
{
  public string RoomName { get; set; }
}

public class Conference : IConference
{
  private IOLabConfiguration _configuration { get; }
  private OLabDBContext _dbContext { get; }
  private TTalkDBContext _ttalkDbContext { get; }
  private readonly SemaphoreSlim _topicSemaphore = new SemaphoreSlim(1, 1);
  private readonly SemaphoreSlim _atriumSemaphore = new SemaphoreSlim(1, 1);

  private IOLabLogger _logger { get; }

  public uint Id { get; set; }
  public string Name { get; set; } = null!;

  public IOLabConfiguration Configuration { get { return _configuration; } }
  public IOLabLogger Logger { get { return _logger; } }
  public TTalkDBContext TTDbContext { get { return _ttalkDbContext; } }
  public SemaphoreSlim TopicSemaphore { get { return _topicSemaphore; } }
  public SemaphoreSlim AtriumSemaphore { get { return _atriumSemaphore; } }

  public Conference()
  {

  }

  public Conference(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    TTalkDBContext ttalkDbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(ttalkDbContext).NotNull(nameof(ttalkDbContext));

    _logger = OLabLogger.CreateNew<Conference>(loggerFactory);
    _configuration = configuration;
    _dbContext = dbContext;
    _ttalkDbContext = ttalkDbContext;

    // load the initial conference (for now, it's assumed to
    // only be one of, for now)
    var physConference = TTDbContext.TtalkConferences
      .FirstOrDefault() ?? throw new Exception("System conference record not defined");

    Id = physConference.Id;
    Name = physConference.Name;

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
