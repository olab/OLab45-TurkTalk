using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;

namespace OLab.TurkTalk.Endpoints.Interface;
public interface IConference
{
  IOLabConfiguration Configuration { get; }
  IOLabLogger Logger { get; }
  TTalkDBContext TTDbContext { get; }
  SemaphoreSlim TopicSemaphore { get; }
  SemaphoreSlim AtriumSemaphore { get; }

  uint Id { get; set; }
  string Name { get; set; }

  //Task<ConferenceTopic> GetTopicAsync(
  //TtalkTopicRoom physRoom,
  //bool createInDb = true);
}