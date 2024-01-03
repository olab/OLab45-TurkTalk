using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints.Interface;

public interface ITopicAtrium
{
  bool? AddLearner(TopicParticipant dtoLearner, DispatchedMessages messageQueue);
  bool Contains(TopicParticipant learner);
  TopicParticipant Get(TopicParticipant learner);
  IList<TopicParticipant> GetLearners();
  void Load(IList<TopicParticipant> participants);
  bool Remove(string connectionId);
  bool Remove(TopicParticipant learner);
}