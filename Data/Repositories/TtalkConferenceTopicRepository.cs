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

  public async Task<TtalkConferenceTopic> GetByQuestionIdAsync(
  TTalkDBContext dbContext,
  uint questionId)
  {
    var physTopic = await dbContext
      .TtalkConferenceTopics
      .Include(x => x.TtalkTopicParticipants)
      .Include(x => x.TtalkTopicRooms)
      .FirstOrDefaultAsync(x => x.QuestionId == questionId);

    return physTopic;
  }

  //public async Task AddAsync(
  //  TTalkDBContext dbContext,
  //  TtalkConferenceTopic phys,
  //  bool commit = true)
  //{
  //  await dbContext
  //    .TtalkConferenceTopics
  //    .AddAsync(phys);

  //  if (commit)
  //    await dbContext.SaveChangesAsync();
  //}

  //public async Task UpdateAsync(
  //  TTalkDBContext dbContext,
  //  TtalkConferenceTopic phys,
  //  bool commit = true)
  //{
  //  dbContext
  //    .TtalkConferenceTopics
  //    .Update(phys);

  //  if (commit)
  //    await dbContext.SaveChangesAsync();
  //}
}
