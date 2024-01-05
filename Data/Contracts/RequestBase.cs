using OLab.TurkTalk.Data.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Data.Contracts;
public class RequestBase
{
  public string UserKey { get; set; }
  public UserToken UserToken { get; set; }
  public string ConnectionId { get; set; }

  public void DecryptAndRefreshUserToken(string secret)
  {
    UserToken = new UserToken().DecryptToken( secret, UserKey );
  }
}
