using Dawn;
using Microsoft.Azure.Functions.Worker;
using OLab.Access;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  /// <summary>
  /// Generate onAuthenticated message for participant
  /// </summary>
  /// <param name="configuration">OLab configuration</param>
  /// <param name="sessionId">Session Id</param>
  /// <param name="connectionId">Connection Id</param>
  /// <param name="auth">OLabAuthentication</param>
  /// <returns>SignalRMessageAction</returns>
  public async Task<SignalRMessageAction> OnAuthenticatedAsync(
    IOLabConfiguration configuration,
    string sessionId,
    string connectionId,
    OLabAuthentication auth)
  {
    try
    {
      Guard.Argument(configuration, nameof(configuration)).NotNull();
      Guard.Argument(auth, nameof(auth)).NotNull();

      // get/create participant
      var physParticipant = await GetCreateParticipantAsync(
        auth,
        sessionId,
        connectionId);

      _logger.LogInformation($"connection {physParticipant.ConnectionId} authenticated and registered as {physParticipant.UserId}.");

      // signal new connection to participant
      return new OnAuthenticatedMethod(
          configuration,
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
