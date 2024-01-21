using Dawn;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class OLabHelper
{
  public readonly IOLabLogger Logger;
  public readonly DatabaseUnitOfWork DbUnitOfWork;

  public OLabHelper()
  {
  }

  public OLabHelper(
    IOLabLogger logger,
    DatabaseUnitOfWork dbUnitOfWork)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(dbUnitOfWork).NotNull(nameof(dbUnitOfWork));

    Logger = logger;
    DbUnitOfWork = dbUnitOfWork;
  }

  public void CommitChanges()
  {
    DbUnitOfWork.Save();
  }

}
