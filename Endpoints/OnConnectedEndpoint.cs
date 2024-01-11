using Dawn;
using Microsoft.Azure.Functions.Worker;
using OLab.Access;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public SignalRMessageAction OnConnected(
    IOLabConfiguration configuration, 
    string connectionId, 
    OLabAuthentication auth)
  {
    try
    {
      Guard.Argument(configuration, nameof(configuration)).NotNull();
      Guard.Argument(connectionId, nameof(connectionId)).NotEmpty();
      Guard.Argument(auth, nameof(auth)).NotNull();

      // signal new connection to participant
      return new NewConnectionMethod(
          configuration,
          connectionId,
          0,
          auth).MessageAction();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "AssignLearnerAsync");
      throw;
    }
    finally
    {
      dbUnitOfWork.Save();
    }
  }
}
