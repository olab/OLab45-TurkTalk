using Dawn;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Data.BusinessObjects;
using OLab.TurkTalk.Data.BusinessObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Data.Dtos;
public class ConferenceDto
{
  private IOLabConfiguration _configuration { get; }
  private OLabDBContext _dbContext { get; }
  private TTalkDBContext _ttalkDbContext { get; }
  private IOLabLogger _logger { get; }
  private readonly IDictionary<string, ConferenceTopic> _topics;
  private static readonly Mutex _topicMutex = new Mutex();

  public uint Id { get; set; }
  public string Name { get; set; } = null!;

  public ConferenceDto()
  {
    throw new NotImplementedException();
  }

  public ConferenceDto(
    ILoggerFactory loggerFactory,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    TTalkDBContext ttalkDbContext)
  {
    Guard.Argument(loggerFactory).NotNull(nameof(loggerFactory));
    Guard.Argument(configuration).NotNull(nameof(configuration));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));
    Guard.Argument(ttalkDbContext).NotNull(nameof(ttalkDbContext));

    _configuration = configuration;
    _dbContext = dbContext;
    _ttalkDbContext = ttalkDbContext;

    _logger = OLabLogger.CreateNew<ConferenceDto>(loggerFactory);
    _topics = new ConcurrentDictionary<string, ConferenceTopic>();

    var dbConference = ttalkDbContext.TtalkConferences.FirstOrDefault();
    if (dbConference == null)
      throw new Exception("System conference not defined");

    Id = dbConference.Id;
    Name = dbConference.Name;

    _logger.LogInformation($"conference'{Name}' ({Id}) loaded from conference");

  }

  public IList<ConferenceTopic> Topics
  {
    get { return _topics.Values.ToList(); }
  }

  /// <summary>
  /// Get/create new conference topic
  /// </summary>
  /// <param name="name">Topic to retrieve/create</param>
  /// <param name="createInDb">Optional flag to create in database, if not found</param>
  /// <returns></returns>
  protected async Task<TtalkConferenceTopic> GetTopicAsync(string name, bool createInDb = true)
  {
    try
    {
      _topicMutex.WaitOne();

      if (_topics.TryGetValue(name, out var topic))
      {
        _logger.LogInformation($"topic '{name}' found in conference");
        return topic;
      }

      topic = await _ttalkDbContext
        .TtalkConferenceTopics
        .FirstOrDefaultAsync(x => x.Name == name);

      if (topic != null)
      {
        _logger.LogInformation($"topic '{name}' found in database");
        return topic;
      }

      if (createInDb)
      {
        topic = new TtalkConferenceTopic
        {
          Name = name,
          ConferenceId = Id
        };

        var newTopic = await _ttalkDbContext.TtalkConferenceTopics.AddAsync(topic);
        topic = newTopic.Entity;

        _logger.LogInformation($"topic '{name}' ({topic.Id}) created in database");
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
      _topicMutex.ReleaseMutex();
    }

  }
}
