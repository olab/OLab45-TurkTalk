using OLab.Api.TurkTalk.BusinessObjects;
using OLab.Api.TurkTalk.Commands;
using OLab.Common.Interfaces;
using System.Threading;

namespace OLab.TurkTalk.ParticipantSimulator
{
  public partial class TopicAtriumThread
  {
    private TopicAtrium _atrium;
    private readonly IOLabLogger _logger;

    public TopicAtriumThread(IOLabLogger logger)
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

        _logger.LogInformation($"transmitting atrium update: {_atrium.AtriumLearners.Count}");

        Thread.Sleep(7500);
      }
      while (true);
    }

  }
}