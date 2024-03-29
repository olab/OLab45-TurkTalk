using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.BusinessObjects;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;
internal class ModeratorStatusMethod : TTalkMethod
{
  public TopicParticipantDto Participant { get; set; }
  public bool Connected { get; set; }

  public ModeratorStatusMethod(
    IOLabConfiguration configuration,
    TtalkTopicRoom physRoom, 
    TtalkTopicParticipant physModerator,
    bool connected) : base(
      configuration, 
      physRoom.RoomLearnersChannel, 
      "moderatorstatus")
  {
    Participant = new TopicParticipantDto(physModerator);
    Connected = connected;
  }

  public override object Arguments()
  {
    return this;
  }
}