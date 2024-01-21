using Dawn;
using Microsoft.Azure.Functions.Worker;
using OLab.Access;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{

  public SignalRMessageAction OnNotAuthenticated(
    IOLabConfiguration configuration,
    string connectionId,
    string exceptionMessage)
  {
    try
    {
      Guard.Argument(configuration, nameof(configuration)).NotNull();
      Guard.Argument(connectionId, nameof(connectionId)).NotEmpty();
      Guard.Argument(exceptionMessage, nameof(exceptionMessage)).NotEmpty();

      // signal new connection to participant
      return new OnAuthenticatedMethod(
          configuration,
          connectionId,
          exceptionMessage).MessageAction();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "OnAuthenticated");
      throw;
    }
  }

  public SignalRMessageAction OnAuthenticated(
    IOLabConfiguration configuration,
    string connectionId,
    TtalkTopicParticipant physParticipant,
    OLabAuthentication auth)
  {
    try
    {
      Guard.Argument(configuration, nameof(configuration)).NotNull();
      Guard.Argument(connectionId, nameof(connectionId)).NotEmpty();
      Guard.Argument(auth, nameof(auth)).NotNull();

      // signal new connection to participant
      return new OnAuthenticatedMethod(
          configuration,
          connectionId,
          0,
          physParticipant,
          auth).MessageAction();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "OnAuthenticated");
      throw;
    }
  }
}
