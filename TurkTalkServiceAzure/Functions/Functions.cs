using Azure.Core.Serialization;
using Dawn;
using DotnetIsolated_ClassBased;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.SignalR.Management;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Service.Azure.BusinessObjects;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.Functions;

public partial class Functions
{
  private static readonly ObjectSerializer JsonObjectSerializer
    = new JsonObjectSerializer(new(JsonSerializerDefaults.Web));

  protected readonly IOLabLogger _logger;
  private readonly IHubContextStore _hubContextStore;
  private readonly OLabDBContext _dbContext;

  private ServiceHubContext MessageHubContext => _hubContextStore.MessageHubContext;

  public Functions(
    IOLabLogger logger,
    IHubContextStore hubContextStore,
    OLabDBContext dbContext)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(hubContextStore).NotNull(nameof(hubContextStore));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    _logger = logger;
    _hubContextStore = hubContextStore;
    _dbContext = dbContext;
  }

  [Function("index")]
  public HttpResponseData GetWebPage(
    [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData req)
  {
    var response = req.CreateResponse(HttpStatusCode.OK);
    response.WriteString(File.ReadAllText("content/index.html"));
    response.Headers.Add("Content-Type", "text/html");
    return response;
  }

  [Function("negotiate")]
  public async Task<HttpResponseData> Negotiate(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
  {
    _logger.LogInformation("C# HTTP trigger function processed a request.");

    var negotiateResponse = await MessageHubContext.NegotiateAsync(new() { UserId = req.Headers.GetValues("userId").FirstOrDefault() });
    var response = req.CreateResponse();
    // We need to make sure the response JSON naming is camelCase, otherwise SignalR client can't recognize it.
    await response.WriteAsJsonAsync(negotiateResponse, JsonObjectSerializer);
    return response;
  }


  [Function("OnConnected")]
  public Task OnConnected(
    [SignalRTrigger("Hub", "connections", "connected")]
        SignalRInvocationContext invocationContext)
  {
    invocationContext.Headers.TryGetValue("Authorization", out var auth);
    _logger.LogInformation($"{invocationContext.ConnectionId} has connected");
    return MessageHubContext.Clients.All.SendAsync("newConnection", new NewConnection(invocationContext.ConnectionId, auth));
  }

  [Function("Broadcast")]
  public Task Broadcast(
    [SignalRTrigger("Hub", "messages", "broadcast", "message")]
        SignalRInvocationContext invocationContext,
    string message)
  {
    return MessageHubContext.Clients.All.SendAsync("newMessage", new NewMessage(invocationContext, message));
  }

  [Function("SendToGroup")]
  public Task SendToGroup(
    [SignalRTrigger("Hub", "messages", "SendToGroup", "groupName", "message")]
        SignalRInvocationContext invocationContext,
    string groupName,
    string message)
  {
    return MessageHubContext.Clients.Group(groupName).SendAsync("newMessage", new NewMessage(invocationContext, message));
  }

  [Function("SendToUser")]
  public Task SendToUser(
    [SignalRTrigger("Hub", "messages", "SendToUser", "userName", "message")]
        SignalRInvocationContext invocationContext, string userName, string message)
  {
    return MessageHubContext.Clients.User(userName).SendAsync("newMessage", new NewMessage(invocationContext, message));

  }

  [Function("SendToConnection")]
  public Task SendToConnection(
    [SignalRTrigger("Hub", "messages", "SendToConnection", "connectionId", "message")]
        SignalRInvocationContext invocationContext,
    string connectionId,
    string message)
  {
    return MessageHubContext.Clients.Client(connectionId).SendAsync("newMessage", new NewMessage(invocationContext, message));
  }

  [Function("JoinGroup")]
  [SignalROutput(HubName = "Hub")]
  public Task JoinGroup(
    [SignalRTrigger("Hub", "messages", "JoinGroup", "connectionId", "groupName")]
        SignalRInvocationContext invocationContext,
    string connectionId,
    string groupName)
  {
    return MessageHubContext.Groups.AddToGroupAsync(connectionId, groupName);
  }

  [Function("LeaveGroup")]
  [SignalROutput(HubName = "Hub")]
  public Task LeaveGroup(
    [SignalRTrigger("Hub", "messages", "LeaveGroup", "connectionId", "groupName")]
        SignalRInvocationContext invocationContext,
    string connectionId,
    string groupName)
  {
    return MessageHubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
  }

  [Function("JoinUserToGroup")]
  [SignalROutput(HubName = "Hub")]
  public Task JoinUserToGroup(
    [SignalRTrigger("Hub", "messages", "JoinUserToGroup", "userName", "groupName")]
        SignalRInvocationContext invocationContext,
    string userName,
    string groupName)
  {
    return MessageHubContext.UserGroups.AddToGroupAsync(userName, groupName);
  }

  [Function("LeaveUserFromGroup")]
  [SignalROutput(HubName = "Hub")]
  public Task LeaveUserFromGroup(
    [SignalRTrigger("Hub", "messages", "LeaveUserFromGroup", "userName", "groupName")]
        SignalRInvocationContext invocationContext, string userName, string groupName)
  {
    return MessageHubContext.UserGroups.RemoveFromGroupAsync(userName, groupName);
  }

  [Function("OnDisconnected")]
  public void OnDisconnected(
    [SignalRTrigger("Hub", "connections", "disconnected")]
        SignalRInvocationContext invocationContext)
  {
    _logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");
  }
}
