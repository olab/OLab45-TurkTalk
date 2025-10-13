using System.Collections.Generic;

namespace TurkTalkSvc.Interface;

public interface IUserContext
{
  public string SessionId
  {
    get;
    set;
  }

  public string Role
  {
    get;
    set;
  }

  public uint UserId
  {
    get;
    set;
  }

  public string UserName
  {
    get;
    set;
  }

  public string IPAddress
  {
    get;
    set;
  }
  public string Issuer
  {
    get;
    set;
  }

  string ReferringCourse { get; }

  public IList<string> UserRoles { get; }

  public string ToString();
}