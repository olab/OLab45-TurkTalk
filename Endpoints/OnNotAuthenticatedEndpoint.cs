using Dawn;
using Microsoft.Azure.Functions.Worker;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using OLab.Access;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.BusinessObjects;
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

      _logger.LogInformation($"connection {connectionId} not authenicated. reason {exceptionMessage}");

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
}
