using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.Interface;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Test;

public class TopicAtriumTest
{
  //private ITopicAtrium _atrium;
  private IOLabLogger _logger;
  private IOLabConfiguration _configuration;

  public TopicAtriumTest()
  {
    _logger = new OLabLogger();


    var myConfiguration = new List<KeyValuePair<string, string?>>
    {
      new KeyValuePair<string, string?>("AppSettings:Audience", "Audience"),
      new KeyValuePair<string, string?>("AppSettings:FileStorageConnectionString", "Audience"),
      new KeyValuePair<string, string?>("AppSettings:FileStorageRoot", "Audience"),
      new KeyValuePair<string, string?>("AppSettings:FileStorageType", "Audience"),
      new KeyValuePair<string, string?>("AppSettings:FileStorageUrl", "Audience"),
      new KeyValuePair<string, string?>("AppSettings:Issuer", "Audience"),
      new KeyValuePair<string, string?>("AppSettings:Secret", "Audience"),
      new KeyValuePair<string, string?>("AppSettings:SignalREndpoint", "Audience"),
      new KeyValuePair<string, string?>("AppSettings:TokenExpiryMinutes", "35")
    };

    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(myConfiguration)
        .Build();
    _configuration = new OLabConfiguration(NullLoggerFactory.Instance, configuration);

    //_atrium = new TopicAtrium("test", _logger, _configuration);
  }

  private Endpoints.BusinessObjects.TopicParticipant GenerateParticipant(uint index)
  {
    var participant = new Endpoints.BusinessObjects.TopicParticipant
    {
      Id = index,
      ConnectionId = $"connid{index}",
      NickName = $"nickname{index}",
      SessionId = Guid.NewGuid().ToString(),
      TokenIssuer = "xunit",
      TopicId = index,
      UserId = index.ToString(),
      UserName = $"userName{index}"
    };

    return participant;
  }

  //[Fact]
  //public async Task AddLearnerShouldGetSaved()
  //{
  //  DispatchedMessages messageQueue = new DispatchedMessages(_logger);

  //  await _atrium.AddLearnerAsync(
  //    GenerateParticipant(1),
  //    messageQueue);

  //    Assert.Equal(2, messageQueue.Messages.Count);
  //}
}