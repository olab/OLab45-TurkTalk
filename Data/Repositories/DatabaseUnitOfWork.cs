using Dawn;
using OLab.Api.Model;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.BusinessObjects;

namespace OLab.TurkTalk.Data.Repositories;
public class DatabaseUnitOfWork : IDisposable
{
  private TtalkConferenceTopicRepository _conferenceTopicRepository;
  private TtalkTopicParticipantRepository _topicParticipantRepository;
  private TtalkTopicRoomRepository _topicRoomRepository;

  private bool disposed = false;

  public IOLabLogger Logger { get; }
  public TTalkDBContext DbContextTT { get; }
  public OLabDBContext DbContextOLab { get; }

  public DatabaseUnitOfWork(
    IOLabLogger logger,
    TTalkDBContext ttDbContext,
    OLabDBContext dbContext)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(ttDbContext).NotNull(nameof(ttDbContext));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    Logger = logger;
    DbContextTT = ttDbContext;
    DbContextOLab = dbContext;
  }

  public TtalkTopicRoomRepository TopicRoomRepository
  {
    get
    {
      _topicRoomRepository ??=
        new TtalkTopicRoomRepository(this);
      return _topicRoomRepository;
    }
  }

  public TtalkConferenceTopicRepository ConferenceTopicRepository
  {
    get
    {
      _conferenceTopicRepository ??=
        new TtalkConferenceTopicRepository(this);
      return _conferenceTopicRepository;
    }
  }

  public TtalkTopicParticipantRepository TopicParticipantRepository
  {
    get
    {
      _topicParticipantRepository ??=
        new TtalkTopicParticipantRepository(this);
      return _topicParticipantRepository;
    }
  }

  public void Save()
  {
    DbContextTT.SaveChanges();
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!disposed)
    {
      if (disposing)
      {
        DbContextTT.Dispose();
      }
    }
    disposed = true;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }
}
