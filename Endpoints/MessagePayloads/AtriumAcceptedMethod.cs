using Dawn;
using OLab.Common.Interfaces;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class AtriumAcceptedMethod : TTalkMethod
{
  //  payload properties
  public string TopicName { get; set; }
  public bool WasAdded { get; set; }
  public bool ModeratorPresent { get; }

  public AtriumAcceptedMethod(
    IOLabConfiguration configuration,
    string groupName,
    string topicName,
    int numberOfModerators,
    bool wasAdded) : base(
      configuration,
      groupName,
      "atriumaccepted")
  {
    Guard.Argument(topicName).NotEmpty(nameof(topicName));

    TopicName = topicName;
    WasAdded = wasAdded;
    ModeratorPresent = numberOfModerators > 0;
  }

  public override object Arguments()
  {
    return this;
  }

  public override string ToString()
  {
    return $"{TopicName} {WasAdded}";
  }
}
