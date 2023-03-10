using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OLabWebAPI.Services.TurkTalk.Contracts;
using OLabWebAPI.TurkTalk.BusinessObjects;
using OLabWebAPI.TurkTalk.Contracts;
using System;
using System.Text.Json;

namespace OLabWebAPI.Services.TurkTalk
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TurkTalkHub : Hub
  {
    /// <summary>
    /// Moderator has request a learner to jump to a node
    /// </summary>
    /// <param name="payload">Jump node payload</param>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public void JumpNode(JumpNodePayload payload)
    {
      try
      {
        _logger.LogInformation($"OnJumpNodeCommand '{JsonSerializer.Serialize(payload)}'");

        // get or create a topic
        Topic topic = _conference.GetCreateTopic(payload.Envelope.From.TopicName, false);
        if (topic == null)
          return;

        // dispatch message
        topic.Conference.SendMessage(
          new JumpNodeMethod(payload));

      }
      catch (Exception ex)
      {
        _logger.LogError($"OnJumpNodeCommand exception: {ex.Message}");
      }
    }
  }
}
