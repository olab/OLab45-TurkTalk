using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Data.Models;
public partial class TtalkTopicParticipant
{
  public override string ToString()
  {
    return $"{Id}: {UserId}//{UserName}//{TokenIssuer}//{SessionId}";

  }
}
