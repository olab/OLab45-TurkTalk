using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using OLab.TurkTalk.Endpoints.Utils;

namespace OLab.TurkTalk.Endpoints.Interface;
public interface IConference
{
    IOLabConfiguration Configuration { get; }
    Task<ConferenceTopic> GetTopicAsync(
      TtalkTopicRoom physRoom, 
      bool createInDb = true);
}