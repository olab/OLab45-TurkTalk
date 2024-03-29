using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints.Interface;

public interface ITopicAtrium
{
  Task<bool?> AddLearnerAsync(BusinessObjects.TopicParticipant dtoLearner, DispatchedMessages messageQueue);
  Task<bool> ContainsAsync(BusinessObjects.TopicParticipant learner, bool doWait = true);
  Task<BusinessObjects.TopicParticipant> Get(BusinessObjects.TopicParticipant learner);
  Task<IList<BusinessObjects.TopicParticipant>> GetLearnersAsync();
  Task LoadAsync(IList<BusinessObjects.TopicParticipant> participants);
  Task<bool> RemoveAsync(string connectionId);
  Task<bool> RemoveAsync(BusinessObjects.TopicParticipant learner);
}