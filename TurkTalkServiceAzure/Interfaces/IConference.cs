using OLab.TurkTalk.Service.Azure.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.Interfaces;
public interface IConference
{
  Topic GetTopic(string name, bool create = true);
}
