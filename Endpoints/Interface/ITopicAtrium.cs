using OLab.Common.Interfaces;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints.Interface;

public interface ITopicAtrium
{
  Task<bool?> AddLearnerAsync(TopicParticipant dtoLearner, DispatchedMessages messageQueue);
  Task<bool> ContainsAsync(TopicParticipant learner, bool doWait = true);
  Task<TopicParticipant> Get(TopicParticipant learner);
  Task<IList<TopicParticipant>> GetLearnersAsync();
  Task LoadAsync(IList<TopicParticipant> participants);
  Task<bool> RemoveAsync(string connectionId);
  Task<bool> RemoveAsync(TopicParticipant learner);
}