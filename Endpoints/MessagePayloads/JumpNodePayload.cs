using OLab.Api.TurkTalk.Contracts;
using OLab.TurkTalk.Endpoints.BusinessObjects;

namespace OLab.Api.Common.Contracts;

public class JumpNodePayload
{
  public TurkTalk.Contracts.Envelope Envelope { get; set; }
  public TargetNode Data { get; set; }
  //public SessionInfo Session { get; set; }

  public JumpNodePayload(TurkTalk.Contracts.Envelope envelope, TargetNode data /* SessionInfo session */ )
  {
    Envelope = envelope;
    Data = data;
    //Session = session;
  }

}
