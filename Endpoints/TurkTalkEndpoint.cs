using Dawn;
using DocumentFormat.OpenXml.Presentation;
using OLab.Api.Common.Contracts;
using OLab.Common.Interfaces;
using OLab.Data.BusinessObjects;
using OLab.TurkTalk.Data.Models;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  protected readonly OLabDBContext dbContext;
  protected readonly TTalkDBContext ttalkDbContext;
  protected IOLabLogger Logger;
  protected readonly IOLabConfiguration _configuration;

  public TurkTalkEndpoint(
    IOLabLogger logger,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    TTalkDBContext ttalkDbContext,
    TtalkConference conference)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(ttalkDbContext).NotNull(nameof(ttalkDbContext));

    this.dbContext = dbContext;
    this.ttalkDbContext = ttalkDbContext;
    _configuration = configuration;

    Logger = logger;
  }
}
