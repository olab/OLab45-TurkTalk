using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.BusinessObjects;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;
internal class LearnerStatusMethod : TTalkMethod
{
  public TopicParticipantDto Participant { get; set; }
  public bool Connected { get; set; }

  public LearnerStatusMethod(
    IOLabConfiguration configuration, 
    TtalkTopicRoom physRoom, 
    TtalkTopicParticipant participant,
    bool connected) : base( 
      configuration, 
      physRoom.RoomModeratorChannel, 
      "learnerstatus")
  {
    Participant = new TopicParticipantDto(participant);
    Connected = connected;
  }

  public override object Arguments()
  {
    return this;
  }
}