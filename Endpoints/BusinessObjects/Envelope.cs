using OLab.TurkTalk.Endpoints.Utils;

namespace OLab.TurkTalk.Endpoints.BusinessObjects
{
  public class Envelope
  {
    public string To { get; set; }
    public UserToken From { get; set; }

    public Envelope()
    {
      From = new UserToken();
    }
  }
}