using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.TurkTalk.Data;
using OLab.TurkTalk.Endpoints.BusinessObjects;

namespace OLab.TurkTalk.Endpoints.Interface;
public interface IConference
{
  IOLabConfiguration Configuration { get; }
  IOLabLogger Logger { get; }
  TTalkDBContext DbContextTtalk { get; }
  ConferenceTopicHelper TopicHelper {  get; }
  SemaphoreManager Semaphores { get; }

  uint Id { get; set; }
  string Name { get; set; }
}