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
  public IList<TtalkTopicParticipant> Participants
    = new List<TtalkTopicParticipant>();

  public TopicParticipantHelper(
    IOLabLogger logger,
    DatabaseUnitOfWork dbUnitOfWork) : base(logger, dbUnitOfWork)
  {
  }

  /// <summary>
  /// Load participants from a topic ic
  /// </summary>
  /// <param name="topicId">Topic id</param>
  public void LoadByTopicId(uint topicId)
  {
    Participants = DbUnitOfWork
      .TopicParticipantRepository
      .GetParticipantsForTopic(topicId).ToList();
  }

  /// <summary>
  /// Load participants from all already loaded topic
  /// </summary>
  /// <param name="phys">Topic object</param>
  public void LoadFromTopic(TtalkConferenceTopic phys)
  {
    Participants = phys.TtalkTopicParticipants.ToList();
  }

  /// <summary>
  /// Retrieve a participant by session id
  /// </summary>
  /// <param name="sessionId">Session Id</param>
  /// <param name="allowNull">optionlaly throw an exception</param>
  /// <returns>TtalkTopicParticipant</returns>
  /// <exception cref="Exception">Participant not found</exception>
  public TtalkTopicParticipant GetBySessionId(string sessionId, bool allowNull = true)
  {
    var phys = Participants.FirstOrDefault(x => x.SessionId == sessionId);

    if (phys == null)
      phys = DbUnitOfWork
        .TopicParticipantRepository
        .GetBySessionId(sessionId);

    if ((phys == null) && !allowNull)
      throw new Exception($"unable to find participant for session id '{sessionId}'");

    return phys;
  }

  /// <summary>
  /// Retrieve a participant by connection id
  /// </summary>
  /// <param name="connectionId">Connection Id</param>
  /// <param name="allowNull">optionlaly throw an exception</param>
  /// <returns>TtalkTopicParticipant</returns>
  /// <exception cref="Exception">Participant not found</exception>
  internal TtalkTopicParticipant GetByConnectionId(string connectionId, bool allowNull = true)
  {
    var phys = Participants.FirstOrDefault(x => x.ConnectionId == connectionId);

    if (phys == null)
      phys = DbUnitOfWork
        .TopicParticipantRepository
        .GetByConnectionId(connectionId);

    if ((phys == null) && !allowNull)
      throw new Exception($"unable to find participant for connection id '{connectionId}'");

    return phys;
  }

  public TtalkTopicParticipant GetModerator()
  {
    var phys = Participants.FirstOrDefault(x => x.RoomId.HasValue && !x.SeatNumber.HasValue);
    if (phys == null)
      Logger.LogWarning($"no moderator found for room.");

    return phys;
  }

  public TtalkTopicParticipant UpdateParticipantTopicId(
    string sessionId,
    uint topicId)
  {
    // update connectionId since it's probably changed
    var phys = DbUnitOfWork
      .TopicParticipantRepository
      .UpdateTopicId(sessionId, topicId);

    CommitChanges();

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

    CommitChanges();
  }

  internal void Remove(TtalkTopicParticipant phys)
  {
    DbUnitOfWork
      .TopicParticipantRepository
      .Remove(phys);

    CommitChanges();
  }

  internal async Task InsertAsync(TtalkTopicParticipant phys)
  {
    await DbUnitOfWork
      .TopicParticipantRepository
      .InsertAsync(phys);

    CommitChanges();
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

    CommitChanges();
  }

  internal TtalkTopicParticipant Update(TtalkTopicParticipant physLearner)
  {
    var phys = DbUnitOfWork
      .TopicParticipantRepository
      .Update(physLearner);

    return phys;
  }
}
