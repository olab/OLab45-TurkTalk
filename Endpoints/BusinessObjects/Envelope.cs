using OLab.TurkTalk.Data.Utils;
using OLab.TurkTalk.Endpoints.Utils;

namespace OLab.TurkTalk.Endpoints.BusinessObjects
{
  public class Envelope
  {
    public string ToSessionId { get; set; }
    public UserToken From { get; set; }

    public Envelope()
    {
      From = new UserToken();
    }
  }
}
