using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;

namespace OLab.TurkTalk.Data.Repositories;

public partial class TtalkConferenceTopicRepository : GenericRepository<TtalkConferenceTopic>
{
  public TtalkConferenceTopicRepository(
    DatabaseUnitOfWork databaseUnitOfWork) : base( databaseUnitOfWork)
  {
  }

  public TtalkConferenceTopicRepository(    
    IOLabLogger logger, 
    TTalkDBContext dbContext) : base(logger, dbContext)
  {
  }

  public async Task<TtalkConferenceTopic> GetByNameAsync(
  TTalkDBContext dbContext,
  string roomName)
  {
    var physTopic = await dbContext
      .TtalkConferenceTopics
      .Include(x => x.TtalkTopicParticipants)
      .Include(x => x.TtalkTopicRooms)
      .FirstOrDefaultAsync(x => x.Name == roomName);

    return physTopic;
  }

}
