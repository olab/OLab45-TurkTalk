using Dawn;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Security.Claims;

namespace OLabWebAPI.TurkTalk.BusinessObjects
{
  public class Participant
  {
    private string _topicName;
    private string _userId;
    private string _nickName;
    private int? _roomNumber;
    private string _connectionId;

    public string UserId { get { return _userId; } set { _userId = value; } }
    public string TopicName { get { return _topicName; } set { _topicName = value; } }
    public string NickName { get { return _nickName; } set { _nickName = value; } }
    public string ConnectionId { get { return _connectionId; } set { _connectionId = value; } }
    public string RoomName { get; set; }
    // group name for direct-to-user method messages
    public string CommandChannel { get; set; }
    [JsonProperty("slotIndex")]
    public int SlotIndex { get; set; }

    public int? RoomNumber
    {
      get { return _roomNumber; }
      set { _roomNumber = value; }
    }

    public virtual void AssignToRoom(int index) { throw new NotImplementedException(); }

    public Participant()
    {

    }

    public Participant(HubCallerContext context)
    {
      // extract fields from bearer token
      var identity = (ClaimsIdentity)context.User.Identity;
      var nameClaim = identity.FindFirst("name");
      var userId = identity.FindFirst(ClaimTypes.Name).Value;

      string nickName = "";
      if (nameClaim != null)
        nickName = nameClaim.Value;
      else
        nickName = userId;

      Guard.Argument(context.ConnectionId).NotNull(nameof(context.ConnectionId));
      Guard.Argument(userId).NotNull(nameof(userId));
      Guard.Argument(nickName).NotNull(nameof(nickName));

      _connectionId = context.ConnectionId;
      _nickName = nickName;
      _userId = userId;
    }

    public Participant(string topicName, string userId, string nickName, string connectionId)
    {
      Initialize(topicName, userId, nickName, connectionId);
    }

    private void Initialize(string topicName, string userId, string nickName, string connectionId)
    {
      _connectionId = connectionId;
      _nickName = nickName;

      Guard.Argument(userId).NotEmpty(userId);
      Guard.Argument(topicName).NotEmpty(topicName);

      var topicNameParts = topicName.Split("/");

      // test not a multipart topic, then this learner is for the atrium
      if (topicNameParts.Length == 1)
      {
        _topicName = topicName;
        _userId = userId;
      }
      else
      {
        _topicName = topicNameParts[0];
        // if not room index passed in, this this is an 
        // atrium group, not an actual room
        if (!string.IsNullOrEmpty(topicNameParts[1]))
          _roomNumber = Convert.ToInt32(topicNameParts[1]);
        _userId = topicNameParts[3];
      }
    }

    public bool IsAssignedToRoom()
    {
      return RoomNumber.HasValue;
    }

    public override string ToString()
    {
      return $"{CommandChannel} Id: {ConnectionId}";
    }

  }
}