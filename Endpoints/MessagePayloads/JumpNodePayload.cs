using OLab.TurkTalk.Endpoints.BusinessObjects;

namespace OLab.Api.Common.Contracts;

public class JumpNodePayload
{
  public Envelope Envelope { get; set; }
  public TargetNode Data { get; set; }
  //public SessionInfo Session { get; set; }

  public JumpNodePayload(Envelope envelope, TargetNode data /* SessionInfo session */ )
  {
    Envelope = envelope;
    Data = data;
    //Session = session;
  }

}
