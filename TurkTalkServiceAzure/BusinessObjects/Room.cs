using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Service.Azure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.BusinessObjects;
public class Room : TurkTalkObject, IRoom
{
  public Room(
    IOLabLogger logger, 
    IOLabConfiguration configuration, 
    OLabDBContext dbContext,
    string name) :
    base(logger, configuration, dbContext, name)
  {
  }
}
