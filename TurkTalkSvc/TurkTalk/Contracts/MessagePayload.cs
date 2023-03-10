using OLabWebAPI.TurkTalk.BusinessObjects;

namespace OLabWebAPI.TurkTalk.Contracts
{
  public class Envelope
  {
    public string To { get; set; }
    public Learner From { get; set; }

    public Envelope()
    {
      From = new Learner();
    }
  }

  public class SessionInfo
  {
    public string ContextId { get; set; }
    public uint MapId { get; set; }
    public uint NodeId { get; set; }
    public uint QuestionId { get; set; }
  }

  public class MessagePayload
  {
    public Envelope Envelope { get; set; }
    public string Data { get; set; }
    public SessionInfo Session { get; set; }

    public MessagePayload()
    {
      Session = new SessionInfo();
      Envelope = new Envelope();
    }

    /// <summary>
    /// Construct message to specific Participant
    /// </summary>
    /// <param name="participant">Recipient</param>
    /// <param name="message">Message to send</param>
    public MessagePayload(string commandChannel, string message)
    {
      Session = new SessionInfo();
      Envelope = new Envelope();
      Envelope.To = commandChannel;
      Data = message;
    }

    /// <summary>
    /// Construct message to specific Participant
    /// </summary>
    /// <param name="participant">Recipient</param>
    /// <param name="message">Message to send</param>
    public MessagePayload(Participant participant, string message)
    {
      Session = new SessionInfo();
      Envelope = new Envelope();
      Envelope.To = participant.CommandChannel;
      Data = message;
    }
  }
}
