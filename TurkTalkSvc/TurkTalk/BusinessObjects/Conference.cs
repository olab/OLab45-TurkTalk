using Common.Utils;
using Dawn;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OLab.Api.Services.TurkTalk;
using OLab.Api.TurkTalk.Commands;
using OLab.Api.TurkTalk.Methods;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.Api.TurkTalk.BusinessObjects
{
  public class Conference
  {
    private readonly IDictionary<string, Topic> _topics;
    //public ILogger Logger { get { return _logger; } }
    public IOLabLogger Logger;

    public readonly IHubContext<TurkTalkHub> HubContext;

    public readonly IServiceScopeFactory ScopeFactory;
    private static readonly Mutex topicMutex = new Mutex();

    public Conference(IOLabLogger logger, IHubContext<TurkTalkHub> hubContext, IServiceScopeFactory scopeFactory)
    {
      Guard.Argument(logger).NotNull(nameof(logger));
      Guard.Argument(hubContext).NotNull(nameof(hubContext));
      Guard.Argument(scopeFactory).NotNull(nameof(scopeFactory));

      ScopeFactory = scopeFactory;

      Logger = logger;

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
      Logger.LogDebug($"{ConnectionIdUtils.Shorten(connectionId)}: adding connection to group '{groupName}'");
      await HubContext.Groups.AddToGroupAsync(connectionId, groupName);
    }

    public async Task RemoveConnectionToGroupAsync(string groupName, string connectionId)
    {
      Logger.LogDebug($"{ConnectionIdUtils.Shorten(connectionId)}: removing connection from group '{groupName}'");
      await HubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
    }

    /// <summary>
    /// Send message payload to single client
    /// </summary>
    /// <param name="connectionId">connection id to transmit payload to</param>
    /// <param name="method">message payload</param>
    public void SendMessage(string connectionId, Method method )
    {
      Guard.Argument(connectionId).NotEmpty(connectionId);

      if (method is CommandMethod)
      {
        var commandMethod = method as CommandMethod;
        Logger.LogDebug($"Send message to '{ConnectionIdUtils.Shorten(connectionId)}' ({method.MethodName}/{commandMethod.Command}): '{method.ToJson()}'");
      }
      else
        Logger.LogDebug($"Send message to '{ConnectionIdUtils.Shorten(connectionId)}' ({method.MethodName}): '{method.ToJson()}'");

      HubContext.Clients.Client(connectionId).SendAsync(method.MethodName, method);
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