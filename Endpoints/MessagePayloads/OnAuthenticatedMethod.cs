using Dawn;
using OLab.Access.Interfaces;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Utils;
using System.Security.Claims;

namespace OLab.TurkTalk.Endpoints.MessagePayloads;

public class OnAuthenticatedMethod : TTalkMethod
{
  private readonly IOLabAuthentication _auth;
  public string UserKey { get; set; }
  public string ErrorMessage { get; set; }
  public TopicParticipantDto Participant { get; set; }

  /// <summary>
  /// Authentication succeesed version of method
  /// </summary>
  /// <param name="configuration">OLab configuration</param>
  /// <param name="connectionId">SignalR connection id</param>
  /// <param name="topicId">Topic connected to</param>
  /// <param name="auth">OLab authentication information</param>
  public OnAuthenticatedMethod(
    IOLabConfiguration configuration,
    uint topicId,
    TtalkTopicParticipant physParticipant,
    IOLabAuthentication auth) : base(
      configuration,
      physParticipant.ConnectionId,
      "onauthenticated")
  {
    Guard.Argument(auth).NotNull(nameof(auth));

    _auth = auth;

    UserKey = new UserToken().EncryptToken(
      Configuration.GetAppSettings().Secret,
      _auth.Claims["id"],
      _auth.Claims[ClaimTypes.Name],
      _auth.Claims["name"],
      _auth.Claims["iss"],
      topicId);

    Participant = new TopicParticipantDto(physParticipant);
  }

  /// <summary>
  /// Authentication failed version of method
  /// </summary>
  /// <param name="configuration">OLab configuration</param>
  /// <param name="connectionId">SignalR connection id</param>
  /// <param name="message">Error message</param>
  public OnAuthenticatedMethod(
    IOLabConfiguration configuration,
    string connectionId,
    string message) : base(
      configuration,
      connectionId,
      "onauthenticated")
  {
    Guard.Argument(message, nameof(message)).NotEmpty();

    ErrorMessage = message;
  }

  public override object Arguments()
  {
    return this;
  }

  public override string ToString()
  {
    return UserKey;
  }
}