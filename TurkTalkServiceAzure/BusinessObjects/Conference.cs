using Dawn;
using Microsoft.Identity.Client;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Service.Azure.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.BusinessObjects;
public class Conference : TurkTalkObject, IConference
{
  protected readonly IDictionary<string, Topic> _topics = new Dictionary<string, Topic>();
  private static readonly Mutex topicMutex = new Mutex();

  public Conference(
    IOLabLogger logger, 
    IOLabConfiguration configuration, 
    OLabDBContext dbContext,
    string name) :
    base(logger, configuration, dbContext, name)
  {
    Logger.LogInformation("Conference singleton constructor");
    LoadParticipantState();
  }

  public void LoadParticipantState()
  {
    _topics.Clear();

    var participants = DbContext.TTalkParticipants.ToList();
    Logger.LogInformation($"Loaded {participants.Count} TTalk participant records");

    foreach( var participant in participants )
    {
      var qualifiedName = TurkTalkQualifiedName.Parse(participant);
      var topic = GetTopic(qualifiedName.TopicName);
      var room = topic.GetRoom( qualifiedName.RoomName );  
    }
  }

  /// <summary>
  /// Get/create topic
  /// </summary>
  /// <param name="name">Fully qualified name to get</param>
  /// <param name="create">Optional create, if not exist</param>
  /// <returns>Topic</returns>
  public Topic GetTopic(string name, bool create = true)
  {
    try
    {
      Guard.Argument(name).NotEmpty(name);

      Topic topic = null;
      var qualifiedName = TurkTalkQualifiedName.Parse(name);

      topicMutex.WaitOne();

      // test if topic doesn't exist yet
      if (!_topics.TryGetValue(qualifiedName.TopicName, out topic))
      {
        if (create)
        {
          Logger.LogInformation($"Creating topic '{qualifiedName.TopicName}'");

          topic = new Topic(Logger, Configuration, DbContext, qualifiedName.TopicName);
          _topics.Add(qualifiedName.TopicName, topic );
        }
      }

      return topic;
    }
    finally
    {
      topicMutex.ReleaseMutex();
    }

  }

}
