using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using OLab.TurkTalk.Endpoints.Utils;

namespace OLab.TurkTalk.Endpoints.Interface;
public interface IConference
{
    IList<ConferenceTopic> Topics { get; }

    Task<ConferenceTopic> GetTopicAsync(
      AttendeePayload payload, 
      bool createInDb = true);
}