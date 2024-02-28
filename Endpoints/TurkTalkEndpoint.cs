using Dawn;
using Newtonsoft.Json;
using OLab.Common.Interfaces;
using OLab.Api.Model;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  protected readonly TTalkDBContext ttalkDbContext;

  private readonly IConference _conference;
  private readonly IOLabLogger _logger;
  public DispatchedMessages MessageQueue { get; }

  protected readonly IOLabConfiguration Configuration;

  public ConferenceTopicHelper TopicHelper 
  { 
    get { return _conference.TopicHelper; } 
  }

  public TopicRoomHelper RoomHelper 
  { 
    get { return _conference.TopicHelper.RoomHelper; } 
  }

  public TurkTalkEndpoint(
    IOLabLogger logger,
    IOLabConfiguration configuration,
    IConference conference)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(conference).NotNull(nameof(conference));

    _conference = conference;
    _logger = logger;
    Configuration = configuration;

    MessageQueue = new DispatchedMessages(_logger);
  }
}
