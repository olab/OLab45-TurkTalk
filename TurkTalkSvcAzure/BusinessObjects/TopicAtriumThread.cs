using OLab.Api.TurkTalk.Commands;
using OLab.Api.Utils;
using System.Threading;

namespace OLab.TurkTalk.Service.Azure.BusinessObjects;

public partial class TopicAtriumThread
{
  private TopicAtrium _atrium;
  private readonly OLabLogger _logger;

  public TopicAtriumThread(OLabLogger logger)
  {
    _logger = logger;
  }

  public void RunProc(object param)
  {
    _atrium = param as TopicAtrium;
    do
    {
      // notify all topic moderators of atrium change
      _atrium.Topic.Conference.SendMessage(
        new AtriumUpdateCommand(_atrium.Topic.TopicModeratorsChannel, _atrium.GetContents()));

      _logger.LogDebug($"transmitting atrium update: {_atrium.AtriumLearners.Count}");

      Thread.Sleep(7500);
    }
    while (true);
  }

}