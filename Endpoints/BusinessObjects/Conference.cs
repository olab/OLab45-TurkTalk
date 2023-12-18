using Dawn;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.Models;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.Mappers;
using OLab.TurkTalk.Endpoints.MessagePayloads;
using OLab.TurkTalk.Endpoints.Utils;
using System.Collections.Concurrent;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class Conference : IConference
{
  private IOLabConfiguration _configuration { get; }
  private OLabDBContext _dbContext { get; }
  private TTalkDBContext _ttalkDbContext { get; }
  private IOLabLogger _logger { get; }
  private readonly IDictionary<string, ConferenceTopic> _topics;
  private SemaphoreSlim _topicSemaphore = new SemaphoreSlim(1, 1);

  public uint Id { get; set; }
  public string Name { get; set; } = null!;

  public IOLabConfiguration Configuration { get { return _configuration; } }
  public IOLabLogger Logger { get { return _logger; } }
  public TTalkDBContext TTDbContext { get { return _ttalkDbContext; } }

  public Conference()
  {
    _topics = new ConcurrentDictionary<string, ConferenceTopic>();
  }

  public Conference(
  ILoggerFactory loggerFactory,
  IOLabConfiguration configuration,
  OLabDBContext dbContext,
  TTalkDBContext ttalkDbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(ttalkDbContext).NotNull(nameof(ttalkDbContext));

    _logger = OLabLogger.CreateNew<Conference>(loggerFactory);
    _configuration = configuration;
    _dbContext = dbContext;
    _ttalkDbContext = ttalkDbContext;

    _topics = new ConcurrentDictionary<string, ConferenceTopic>();

    var dbConference = TTDbContext.TtalkConferences.FirstOrDefault();
    if (dbConference == null)
      throw new Exception("System conference not defined");

    Id = dbConference.Id;
    Name = dbConference.Name;

    _logger.LogInformation($"conference'{Name}' ({Id}) loaded from conference");

  }

  public IList<ConferenceTopic> ConferenceTopics
  {
    get { return _topics.Values.ToList(); }
    set
    {
      foreach (var topic in value)
        AddTopic(topic);
    }
  }

  public void AddTopic(ConferenceTopic topic)
  {
    _topics.Add(topic.Name, topic);
  }

  /// <summary>
  /// Get/create new conference topic
  /// </summary>
  /// <param name="topicName">Topic to retrieve/create</param>
  /// <param name="createInDb">Optional flag to create in database, if not found</param>
  /// <returns></returns>
  public async Task<ConferenceTopic> GetTopicAsync(
    AttendeePayload payload,
    bool createInDb = true)
  {
    try
    {
      await _topicSemaphore.WaitAsync();

      var mapper = new ConferenceTopicMapper(_logger);

      if (_topics.TryGetValue(payload.RoomName, out var topic))
      {
        _logger.LogInformation($"topic '{payload.RoomName}' found in conference");
        return topic;
      }

      var physTopic = await TTDbContext
        .TtalkConferenceTopics
        .Include(x => x.TtalkAtriumAttendees)
        .Include(x => x.TtalkTopicRooms)
        .FirstOrDefaultAsync(x => x.Name == payload.RoomName);

      if (physTopic != null)
      {
        _logger.LogInformation($"topic '{payload.RoomName}' found in database");

        // update last used
        physTopic.LastUsedAt = DateTime.UtcNow;
        TTDbContext
          .TtalkConferenceTopics
          .Update(physTopic);
        await _ttalkDbContext.SaveChangesAsync();

        topic = mapper.PhysicalToDto(physTopic);
      }

      else if (createInDb)
      {
        physTopic = new TtalkConferenceTopic
        {
          Name = payload.RoomName,
          ConferenceId = Id,
          CreatedAt = DateTime.UtcNow,
        };

        await _ttalkDbContext.TtalkConferenceTopics.AddAsync(physTopic);
        await _ttalkDbContext.SaveChangesAsync();

        topic.Id = physTopic.Id;

        AddTopic(topic);

        _logger.LogInformation($"topic '{payload.RoomName}' ({topic.Id}) created in database");
      }

      return topic;
    }
    catch (Exception ex)
    {
      _logger.LogError($"GetTopicAsync error: {ex.Message}");
      throw;
    }
    finally
    {
      _topicSemaphore.Release(1);
    }

  }
}
