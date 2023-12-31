using Dawn;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.Data.Models;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.Mappers;

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
  private IOLabLogger _logger { get; }
  private SemaphoreSlim _topicSemaphore = new SemaphoreSlim(1, 1);

  public uint Id { get; set; }
  public string Name { get; set; } = null!;

  public IOLabConfiguration Configuration { get { return _configuration; } }
  public IOLabLogger Logger { get { return _logger; } }
  public TTalkDBContext TTDbContext { get { return _ttalkDbContext; } }

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
    var dbConference = TTDbContext.TtalkConferences
      .FirstOrDefault() ?? throw new Exception("System conference not defined");

    Id = dbConference.Id;
    Name = dbConference.Name;

    Logger.LogInformation($"conference'{Name}' ({Id}) loaded from conference");

  }

  /// <summary>
  /// Get/create new conference topic
  /// </summary>
  /// <param name="topicName">Topic to retrieve/create</param>
  /// <param name="createInDb">Optional flag to create in database, if not found</param>
  /// <returns>ConferenceTopic</returns>
  public async Task<ConferenceTopic> GetTopicAsync(
    uint questionId,
    bool createInDb = true)
  {
    DatabaseUnitOfWork dbUnitOfWork = null;

    try
    {
      // ensure question is valid and is of correct type (ttalk)
      var question = _dbContext.SystemQuestions.FirstOrDefault(x =>
        x.Id == questionId &&
        (x.EntryTypeId == 11 || x.EntryTypeId == 15)) ?? 
        throw new Exception($"question id {questionId} not found/invalid");

      var questionSetting = 
        JsonConvert.DeserializeObject<QuestionSetting>(question.Settings);
      ConferenceTopic topic = new ConferenceTopic(this);

      await SemaphoreLogger.WaitAsync(
        Logger, 
        $"question {questionId}", 
        _topicSemaphore);

      dbUnitOfWork = new DatabaseUnitOfWork(_logger, TTDbContext);
      var physTopic = await dbUnitOfWork
        .ConferenceTopicRepository
        .GetByQuestionIdAsync(TTDbContext, questionId);

      // test if found topic in database
      if (physTopic != null)
      {
        Logger.LogInformation($"topic '{questionSetting.RoomName}' found in database");

        // update last used
        physTopic.LastusedAt = DateTime.UtcNow;
        dbUnitOfWork.ConferenceTopicRepository.Update( physTopic );

        var mapper = new ConferenceTopicMapper(Logger);
        topic = mapper.PhysicalToDto(physTopic, questionSetting.RoomName, this);
        // update the topic atrium 
        topic.Atrium.Load();
      }

      // topic not found in database
      else if (createInDb)
      {
        physTopic = new TtalkConferenceTopic
        {
          QuestionId = questionId,
          ConferenceId = Id,
          CreatedAt = DateTime.UtcNow,
        };

        await dbUnitOfWork.ConferenceTopicRepository.InsertAsync(physTopic);
        // explicit save needed because we need new inserted Id
        dbUnitOfWork.Save();

        var mapper = new ConferenceTopicMapper(Logger);
        topic = mapper.PhysicalToDto(physTopic, questionSetting.RoomName, this);

        Logger.LogInformation($"topic '{questionSetting.RoomName}' ({topic.Id}) created in database");
      }

      return topic;
    }
    catch (Exception ex)
    {
      Logger.LogError($"GetTopicAsync error: {ex.Message}");
      throw;
    }
    finally
    {
      dbUnitOfWork.Save();

      SemaphoreLogger.Release(
        Logger, 
        $"question {questionId}", 
        _topicSemaphore);
    }

  }
}
