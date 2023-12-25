using Dawn;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using System.Text;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class AtriumAcceptedMethod: TTalkMethod
{
  public string TopicName { get; set; }
  public bool WasAdded { get; set; }

  public AtriumAcceptedMethod(
    IOLabConfiguration configuration,
    string connectionId,
    ConferenceTopic topic,
    bool wasAdded ) : base(
      configuration,
      connectionId,
      "atriumaccepted")
  {
    Guard.Argument(topic).NotNull(nameof(topic));
    Guard.Argument(connectionId, nameof(connectionId)).NotEmpty();

    TopicName = topic.Name;
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
