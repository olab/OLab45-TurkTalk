using Dawn;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Extensions.Logging;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Data.Repositories;
public class DatabaseUnitOfWork : IDisposable
{
  private TtalkConferenceTopicRepository _conferenceTopicRepository;
  private TtalkTopicParticipantRepository _topicParticipantRepository;
  private TtalkTopicRoomRepository _topicRoomRepository;

  private bool disposed = false;

  public IOLabLogger Logger { get; }
  public TTalkDBContext DbContext { get; }

  public DatabaseUnitOfWork(
    IOLabLogger logger,
    TTalkDBContext dbContext)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    Logger = logger;
    DbContext = dbContext;
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
    DbContext.SaveChanges();
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!disposed)
    {
      if (disposing)
      {
        DbContext.Dispose();
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
