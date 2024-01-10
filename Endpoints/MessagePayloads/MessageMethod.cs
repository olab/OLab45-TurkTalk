using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;
internal class MessageMethod : TTalkMethod
{
  public MessageMethod(
    IOLabConfiguration configuration) : base(configuration, "", "sendmessage")
  {

  }

  public override object Arguments()
  {
    return this;
  }
}
