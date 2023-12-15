using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.BusinessObjects;

namespace OLab.TurkTalk.Endpoints.Interface;
public interface IConference
{
    IList<ConferenceTopic> Topics { get; }

    Task<ConferenceTopic> GetTopicAsync(string name, bool createInDb = true);
}