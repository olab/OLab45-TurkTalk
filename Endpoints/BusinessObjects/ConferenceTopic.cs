using OLab.TurkTalk.Endpoints.MessagePayloads;
using OLab.TurkTalk.Endpoints.Utils;
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

  public TopicAtrium Atrium;
  public Conference Conference;

  public ConferenceTopic()
  {
    CreatedAt = DateTime.UtcNow;
    LastUsedAt = DateTime.UtcNow;
    Atrium = new TopicAtrium(this);
  }

  public ConferenceTopic(Conference conference) : this()
  {
    Conference = conference;
  }

  /// <summary>
  /// Add a learner to a topic
  /// </summary>
  /// <param name="contextId"></param>
  /// <param name="learner"></param>
  /// <returns></returns>
  public async Task<bool> AddAttendeeAsync(
    AttendeePayload payload,
    TTalkMessageQueue messageQueue)
  {
    var addedToAtrium = Atrium.AddAttendee(payload, messageQueue);
    return true;

  }
}
