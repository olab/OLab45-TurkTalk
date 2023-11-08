using Dawn;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Service.Azure.BusinessObjects;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.BusinessObjects;

public class TurkTalkObject
{
  protected readonly IOLabLogger Logger;
  protected readonly IOLabConfiguration Configuration;
  protected OLabDBContext DbContext { get; }
  protected string Name { get; private set; }

  public TurkTalkObject(
    IOLabLogger logger,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    string name)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(name).NotEmpty(nameof(name));

    Logger = logger;
    Configuration = configuration;
    DbContext = dbContext;
    Name = name;
  }

}
