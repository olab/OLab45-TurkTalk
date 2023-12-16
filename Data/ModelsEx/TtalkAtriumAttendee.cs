using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Data.Models;
public partial class TtalkAtriumAttendee
{
  public TtalkAtriumAttendee(AttendeePayload attendee)
  {
    TokenIssuer = attendee.UserToken.TokenIssuer;
    TopicId = Topic.Id;
    UserId = attendee.UserToken.UserId.ToString();
    UserName = attendee.UserToken.UserName;
  }
}
