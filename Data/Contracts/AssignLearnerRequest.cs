using OLab.TurkTalk.Data.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Data.Contracts;
public class AssignLearnerRequest : RequestBase
{
  public string LearnerSessionId { get; set; }
  public string ModeratorSessionId { get; set; }
}
