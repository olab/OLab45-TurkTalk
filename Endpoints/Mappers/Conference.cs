using OLab.Common.Interfaces;
using OLab.Data.Mappers;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.Mappers;

public class ConferenceMapper : OLabMapper<TtalkConference, Conference>
{
  public ConferenceMapper(
    IOLabLogger logger,
    bool enableWikiTranslation = true) : base(logger)
  {

  }

}
