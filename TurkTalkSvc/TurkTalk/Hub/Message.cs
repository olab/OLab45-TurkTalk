using Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OLab.Api.TurkTalk.Commands;
using OLab.Api.TurkTalk.Contracts;
using System;
using System.Linq;
namespace OLab.Api.Services.TurkTalk
{
  /// <summary>
  /// 
  /// </summary>
  public partial class TurkTalkHub : Hub
  {
    /// <summary>
    /// Message is received
    /// </summary>
    /// <param name="learner">Learner to remove</param>
    /// <param name="topicName">Topic id</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public void Message(MessagePayload payload)
    {
      try
      {
        _logger.LogInformation($"Message: from '{payload.Envelope.From}', {payload.Session.ContextId}, '{payload.Data}' ({ConnectionIdUtils.Shorten(Context.ConnectionId)})");

        var auth = GetAuthorization(Context.GetHttpContext());
        auth.UserContext.SessionId = payload.Session.ContextId;

        // get or create a topic
        var topic = _conference.GetCreateTopic(payload.Envelope.From.TopicName, false);
        if (topic == null)
          return;

        // dispatch message
        topic.Conference.SendMessage(
          new MessageMethod(payload));

        // add the sender name to the message so we 
        // know who sent it in the log
        var message = $"{payload.Envelope.From.UserId}: {payload.Data}";

        // add message event session activity
        var session = GetSession(Context.GetHttpContext(), auth);

        session.OnQuestionResponse(
          new Dto.QuestionResponsePostDataDto
          {
            QuestionId = payload.Session.QuestionId,
            Value = message,
            NodeId = payload.Session.NodeId
          },
          DbContext.SystemQuestions.FirstOrDefault(x => x.Id == payload.Session.QuestionId));

      }
      catch (Exception ex)
      {
        _logger.LogError($"Message exception: {ex.Message}");
      }
    }
  }
}
