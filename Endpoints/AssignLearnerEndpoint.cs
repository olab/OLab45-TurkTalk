using Dawn;
using OLab.TurkTalk.Data.Contracts;
using OLab.TurkTalk.Endpoints.BusinessObjects;
using OLab.TurkTalk.Endpoints.MessagePayloads;

namespace OLab.TurkTalk.Endpoints;

public partial class TurkTalkEndpoint
{
  public async Task<DispatchedMessages> AssignLearnerAsync(
    AssignLearnerRequest payload)
  {
    try
    {
      Guard.Argument(payload).NotNull(nameof(payload));
      
      // add learner to room
      await RoomHelper.AssignLearnerToRoomAsync(
        MessageQueue,
        payload.LearnerSessionId,
        payload.ModeratorSessionId,
        payload.SeatNumber);

      return MessageQueue;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "AssignLearnerAsync");
      throw;
    }
    finally
    {
      TopicHelper.CommitChanges();
    }
  }
}
