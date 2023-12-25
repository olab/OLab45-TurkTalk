using OLab.TurkTalk.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OLab.TurkTalk.Endpoints.Mappers;
using OLab.Common.Interfaces;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;
public class TopicRoom
{
  public string Name { get; set; }
  public uint Id { get; set; }
  public uint TopicId { get; set; }
  public uint? ModeratorId { get; set; }
  public virtual ICollection<Attendee> Attendees { get; } = new List<Attendee>();
  public virtual TtalkConferenceTopic Topic { get; set; }
  public virtual TtalkTopicParticipant Moderator { get; set; }

  /// <summary>
  /// Creates room with new moderator
  /// </summary>
  /// <param name="topic">Parent topic</param>
  /// <param name="attendee">Attendee (moderator) to assign</param>
  /// <returns>TopicRoom</returns>
  public static async Task<TopicRoom> CreateRoomAsync(
    ConferenceTopic topic,
    TopicParticipant attendee)
  {
    // create new room
    var physRoom = await CreateRoomAsync(topic);

    var physModerator = new TtalkTopicParticipant
    {
      SessionId = attendee.SessionId,
      TopicId = topic.Id,
      UserId = attendee.UserId,
      UserName = attendee.UserName,
      TokenIssuer = attendee.TokenIssuer,
      RoomId = physRoom.Id,
    };

    await topic.Conference.TTDbContext.TtalkTopicParticipants.AddAsync(physModerator);
    await topic.Conference.TTDbContext.SaveChangesAsync();

    physRoom.ModeratorId = physModerator.Id;
    topic.Conference.TTDbContext.TtalkTopicRooms.Update(physRoom);

    await topic.Conference.TTDbContext.SaveChangesAsync();

    topic.Logger.LogInformation($"assigned moderator {physModerator.Id} to '{topic.Name}' room id {physRoom.Id}");

    var mapper = new TopicRoomMapper(topic.Logger);
    var roomDto = mapper.PhysicalToDto(physRoom);
    return roomDto;
  }

  /// <summary>
  /// Creates a new room on the topic
  /// </summary>
  /// <param name="topic">Parent topic</param>
  /// <returns>TopicRoom</returns>
  internal static async Task<TtalkTopicRoom> CreateRoomAsync(ConferenceTopic topic)
  {
    var physRoom = new TtalkTopicRoom
    {
      Name = topic.Name,
      TopicId = topic.Id
    };

    await topic.Conference.TTDbContext.TtalkTopicRooms.AddAsync(physRoom);
    await topic.Conference.TTDbContext.SaveChangesAsync();

    topic.Logger.LogInformation($"created topic '{topic.Name}' room. id {physRoom.Id}");

    return physRoom;
  }
}
