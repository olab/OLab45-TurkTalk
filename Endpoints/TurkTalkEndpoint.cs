using Dawn;
using Newtonsoft.Json;
using OLab.Common.Interfaces;
using OLab.Data.Models;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  protected readonly OLabDBContext dbContext;
  protected readonly TTalkDBContext ttalkDbContext;
  private readonly IConference _conference;
  private readonly IOLabLogger _logger;
  public DispatchedMessages MessageQueue { get; }

  protected readonly DatabaseUnitOfWork dbUnitOfWork;
  protected readonly ConferenceTopicHelper topicHandler;
  protected readonly TopicRoomHelper roomHandler;
  protected readonly IOLabConfiguration configuration;

  public TurkTalkEndpoint(
    IOLabLogger logger,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    TTalkDBContext ttalkDbContext,
    IConference conference)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(ttalkDbContext).NotNull(nameof(ttalkDbContext));

    this.dbContext = dbContext;
    this.ttalkDbContext = ttalkDbContext;
    _conference = conference;
    this.configuration = configuration;

    _logger = logger;

    MessageQueue = new DispatchedMessages(_logger);

    dbUnitOfWork = new DatabaseUnitOfWork(
      _logger,
      ttalkDbContext);

    topicHandler = new ConferenceTopicHelper(
      _logger,
      _conference,
      dbUnitOfWork);

    roomHandler = new TopicRoomHelper(
      _logger,
      topicHandler,
      dbUnitOfWork);
  }

  public string GetTopicNameFromQuestion(uint questionId)
  {
    // ensure question is valid and is of correct type (ttalk)
    var question = dbContext.SystemQuestions.FirstOrDefault(x =>
      x.Id == questionId &&
      (x.EntryTypeId == 11 || x.EntryTypeId == 15)) ??
      throw new Exception($"question id {questionId} not found/invalid");

    var questionSetting =
      JsonConvert.DeserializeObject<QuestionSetting>(question.Settings);

    return questionSetting.RoomName;
  }

}
