using Dawn;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OLabWebAPI.Services.TurkTalk;
using OLabWebAPI.TurkTalk.Commands;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OLabWebAPI.TurkTalk.BusinessObjects
{
  public class Conference
  {
    private readonly ILogger _logger;
    private readonly IDictionary<string, Topic> _topics;
    public ILogger Logger { get { return _logger; } }
    public readonly IHubContext<TurkTalkHub> HubContext;

    public readonly IServiceScopeFactory ScopeFactory;
    private static readonly Mutex topicMutex = new Mutex();

    public Conference(ILogger logger, IHubContext<TurkTalkHub> hubContext, IServiceScopeFactory scopeFactory)
    {
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(hubContext).NotNull(nameof(hubContext));
      Guard.Argument(scopeFactory).NotNull(nameof(scopeFactory));

      ScopeFactory = scopeFactory;

      _logger = logger;
      HubContext = hubContext;
      _topics = new ConcurrentDictionary<string, Topic>();

      logger.LogDebug($"New Conference");
    }

    public IList<Topic> Topics
    {
      get { return _topics.Values.ToList(); }
    }

    public async Task AddConnectionToGroupAsync(Participant group)
    {
      await AddConnectionToGroupAsync(group.CommandChannel, group.ConnectionId);
    }

    public async Task AddConnectionToGroupAsync(string groupName, string connectionId)
    {
      Logger.LogDebug($"Added connection '{connectionId}' to group '{groupName}'");
      await HubContext.Groups.AddToGroupAsync(connectionId, groupName);
    }

    public async Task RemoveConnectionToGroupAsync(string groupName, string connectionId)
    {
      Logger.LogDebug($"Removing connection '{connectionId}' from group '{groupName}'");
      await HubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
    }

    /// <summary>
    /// Send message payload to group
    /// </summary>
    /// <param name="groupName">group name id to transmit payload to</param>
    /// <param name="method">message payload</param>
    public void SendMessage(Method method)
    {
      var groupName = method.CommandChannel;
      Guard.Argument(groupName).NotEmpty(groupName);

      if (method is CommandMethod)
      {
        var commandMethod = method as CommandMethod;
        Logger.LogDebug($"Send message to '{groupName}' ({method.MethodName}/{commandMethod.Command}): '{method.ToJson()}'");
      }
      else
        Logger.LogDebug($"Send message to '{groupName}' ({method.MethodName}): '{method.ToJson()}'");

      HubContext.Clients.Group(groupName).SendAsync(method.MethodName, method);

    }

    /// <summary>
    /// Find/join an unmoderated room for a topic
    /// </summary>
    /// <param name="moderator">Moderator requesting room</param>
    /// <returns>First unmoderated room</returns>
    public Room GetCreateTopicRoom(Moderator moderator)
    {
      Topic topic = GetCreateTopic(moderator.TopicName);
      return topic.GetCreateRoom(moderator);
    }

    /// <summary>
    /// Get/create topic
    /// </summary>
    /// <param name="topicId">Room/Topic Id to get</param>
    /// <param name="create">Optional create, if not exist</param>
    /// <returns>Topic</returns>
    public Topic GetCreateTopic(string roomId, bool create = true)
    {
      try
      {
        Guard.Argument(roomId).NotEmpty(roomId);

        topicMutex.WaitOne();

        // remove any room parts from roomId
        var roomParts = roomId.Split('/');

        // topic id shold be the first
        var topicId = roomParts[0];

        // test if topic doesn't exist yet
        if (!_topics.TryGetValue(topicId, out Topic topic))
        {
          Logger.LogDebug($"Topic '{topicId}' does not already exist");

          if (create)
          {
            _topics.Add(topicId, new Topic(this, topicId));
            topic = _topics[topicId];
          }
          else
            topic = null;
        }
        else
          Logger.LogDebug($"Topic {topicId} already exists");

        return topic;
      }
      finally
      {
        topicMutex.ReleaseMutex();
      }

    }

  }
}