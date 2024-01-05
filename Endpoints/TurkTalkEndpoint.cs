using Dawn;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OLab.Api.Common.Contracts;
using OLab.Common.Interfaces;
using OLab.Data.Models;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  protected readonly OLabDBContext dbContext;
  protected readonly TTalkDBContext ttalkDbContext;
  private readonly IConference _conference;
  private IOLabLogger _logger;
  public DispatchedMessages MessageQueue { get; }

  protected readonly IOLabConfiguration _configuration;

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
    _configuration = configuration;

    _logger = logger;

    MessageQueue = new DispatchedMessages(_logger);
  }

  public TtalkTopicRoom GetRoomFromQuestion(uint questionId)
  {
    // ensure question is valid and is of correct type (ttalk)
    var question = dbContext.SystemQuestions.FirstOrDefault(x =>
      x.Id == questionId &&
      (x.EntryTypeId == 11 || x.EntryTypeId == 15)) ??
      throw new Exception($"question id {questionId} not found/invalid");

    var questionSetting =
      JsonConvert.DeserializeObject<QuestionSetting>(question.Settings);

    var physRoom = ttalkDbContext
      .TtalkTopicRooms
      .Where(x => x.Name == questionSetting.RoomName)
      .FirstOrDefault();

    if ( physRoom == null)
    {
      physRoom = new TtalkTopicRoom
      {
        Name = questionSetting.RoomName
      };
    }

    return physRoom;
  }

}
