using OLab.Api.TurkTalk.BusinessObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class ConferenceTopic
{
  public uint Id { get; set; }
  public string Name { get; internal set; }
  public uint ConferenceId { get; internal set; }
  public DateTime CreatedAt { get; set; }
  public DateTime LastUsedAt { get; set; }
  public TopicAtrium Atrium { get; set; }

  public ConferenceTopic()
  {
    CreatedAt = DateTime.UtcNow;
    LastUsedAt = DateTime.UtcNow;
    Atrium = new TopicAtrium();
  }

  /// <summary>
  /// Add a learner to a topic
  /// </summary>
  /// <param name="contextId"></param>
  /// <param name="learner"></param>
  /// <returns></returns>
  public async Task AddAttendeeAsync(
    string contextId, 
    Learner learner)
  {
    // test if user is connecting fresh
    // meaning we add to atrium
    if (string.IsNullOrEmpty(contextId))
    {
      Atrium.AddAttendee(learner);
      return;
    }




  }
}
