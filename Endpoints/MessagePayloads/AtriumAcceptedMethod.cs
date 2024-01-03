using Dawn;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using System.Text;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class AtriumAcceptedMethod: TTalkMethod
{
  //  payload properties
  public string TopicName { get; set; }
  public bool WasAdded { get; set; }

  public AtriumAcceptedMethod(
    IOLabConfiguration configuration,
    string groupName,
    string topicName,
    bool wasAdded ) : base(
      configuration,
      groupName,
      "atriumaccepted")
  {
    Guard.Argument(topicName).NotEmpty(nameof(topicName));

    TopicName = topicName;
    WasAdded = wasAdded;
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
