using Dawn;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using OLab.Api.Common.Contracts;
using OLab.Common.Interfaces;
using OLab.Data.Models;
using OLab.TurkTalk.Data.Models;
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

}
