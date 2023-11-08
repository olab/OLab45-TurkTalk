using Dawn;
using OLab.Api.Model;
using OLab.Api.TurkTalk.BusinessObjects;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Service.Azure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.BusinessObjects;
public class Topic : TurkTalkObject, ITopic
{
  protected readonly IDictionary<string, Room> _rooms;
  private static readonly Mutex roomMutex = new Mutex();

  public Topic(
    IOLabLogger logger,
    IOLabConfiguration configuration,
    OLabDBContext dbContext,
    string name) :
    base(logger, configuration, dbContext, name)
  {
  }

  /// <summary>
  /// Gets/creates a room for a learner
  /// </summary>
  /// <param name="learner">Learner to search for</param>
  /// <param name="create">Create room is doens't exist</param>
  /// <returns>Existing or new room</returns>
  /// <exception cref="NotImplementedException"></exception>
  public Room GetRoom(Learner learner, bool create = true)
  {
    Guard.Argument(learner).NotNull(nameof(learner));

    var qualifiedName = TurkTalkQualifiedName.Parse(learner.RoomName);
    return GetRoom(qualifiedName, create);

  }

  /// <summary>
  /// Get/create room
  /// </summary>
  /// <param name="roomName">Room name</param>
  /// <param name="create">Create room is doens't exist</param>
  /// <returns>Existing or new room</returns>
  public Room GetRoom(string roomName, bool create = true)
  {
    Guard.Argument(roomName).NotEmpty(nameof(roomName));

    var qualifiedName = TurkTalkQualifiedName.Parse(roomName);
    return GetRoom(qualifiedName, create);
  }

  private Room GetRoom(TurkTalkQualifiedName qualifiedName, bool create)
  {
    try
    {
      Guard.Argument(qualifiedName).NotNull(nameof(qualifiedName));

      Room room = null;

      roomMutex.WaitOne();

      // test if room doesn't exist yet
      if (!_rooms.TryGetValue(qualifiedName.RoomName, out room))
      {
        if (create)
        {
          Logger.LogInformation($"Creating room '{qualifiedName.RoomName}'");

          room = new Room(Logger, Configuration, DbContext, qualifiedName.RoomName);
          _rooms.Add(qualifiedName.RoomName, room);
        }
      }

      return room;
    }
    finally
    {
      roomMutex.ReleaseMutex();
    }
  }
}
