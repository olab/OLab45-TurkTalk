using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using OLab.TurkTalk.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Endpoints.BusinessObjects;

public class TopicParticipantHelper : OLabHelper
{
  public IList<TtalkTopicParticipant> Participants;

  public TopicParticipantHelper(
    IOLabLogger logger,
    DatabaseUnitOfWork dbUnitOfWork) : base(logger, dbUnitOfWork)
  {
  }

  public void LoadByTopicId(uint topicId)
  {
    Participants = DbUnitOfWork
      .TopicParticipantRepository
      .GetParticipantsForTopic(topicId).ToList();
  }

  public TtalkTopicParticipant GetBySessionId(string sessionId, bool allowNull = true)
  {
    var phys = Participants.FirstOrDefault(x => x.SessionId == sessionId);
    if ( ( phys == null ) && !allowNull )
      throw new Exception($"unable to find participant for session '{sessionId}'");

    return phys;
  }

  public TtalkTopicParticipant GetModerator()
  {
    var phys = Participants.FirstOrDefault(x => x.RoomId.HasValue && !x.SeatNumber.HasValue);
    if (phys == null)
      Logger.LogWarning($"no moderator found for room.");

    return phys;
  }

  public void UpdateParticipantConnectionId(
    string sessionId,
    string connectionId)
  {
    // update connectionId since it's probably changed
    DbUnitOfWork
      .TopicParticipantRepository
      .UpdateConnectionId(sessionId, connectionId);

    DbUnitOfWork.Save();
  }

  internal async Task InsertAsync(TtalkTopicParticipant phys)
  {
    await DbUnitOfWork
      .TopicParticipantRepository
      .InsertAsync(phys);

    DbUnitOfWork.Save();
  }

  internal void AssignLearnerToRoom(
    string learnerSessionId, 
    uint roomId, 
    uint seatNumber)
  {
    DbUnitOfWork
      .TopicParticipantRepository
      .AssignToRoom(
        learnerSessionId,
        roomId,
        seatNumber);

    DbUnitOfWork.Save();
  }
}
