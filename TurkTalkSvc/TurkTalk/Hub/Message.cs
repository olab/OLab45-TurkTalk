using Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OLabWebAPI.Data;
using OLabWebAPI.Common.Contracts;
using OLabWebAPI.TurkTalk.BusinessObjects;
using OLabWebAPI.TurkTalk.Commands;
using System;

namespace OLabWebAPI.Services.TurkTalk
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

        // get or create a topic
        Topic topic = _conference.GetCreateTopic(payload.Envelope.From.TopicName, false);
        if (topic == null)
          return;

        // dispatch message
        topic.Conference.SendMessage(
          new MessageMethod(payload));

        UserContext userContext = GetUserContext();
        userContext.Session.SetSessionId(payload.Session.ContextId);

        // add the sender name to the message so we 
        // know who sent it in the log
        var message = $"{payload.Envelope.From.UserId}: {payload.Data}";

        // add message event session activity
        userContext.Session.OnQuestionResponse(
          payload.Session.MapId,
          payload.Session.NodeId,
          payload.Session.QuestionId,
          message);

      }
      catch (Exception ex)
      {
        _logger.LogError($"Message exception: {ex.Message}");
      }
    }
  }
}
