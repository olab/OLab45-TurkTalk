using Dawn;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using OLab.Common.Interfaces;
using OLab.TurkTalk.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Data.Repositories;
public abstract class GenericRepository<TEntity> where TEntity : class
{
  public IOLabLogger Logger { get; }
  public TTalkDBContext DbContext { get; }
  public DbSet<TEntity> dbSet;

  public GenericRepository(
    IOLabLogger logger,
    TTalkDBContext dbContext)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    Logger = logger;
    DbContext = dbContext;
    dbSet = DbContext.Set<TEntity>();
  }

  /// <summary>
  /// Gets records from the database
  /// </summary>
  /// <param name="filter">optional filter criteria</param>
  /// <param name="orderBy">optional order by criteria</param>
  /// <param name="includeProperties">optional fields limiter</param>
  /// <returns>Matched records</returns>
  public virtual IEnumerable<TEntity> Get(
      Expression<Func<TEntity, bool>> filter = null,
      Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
      string includeProperties = "")
  {
    IQueryable<TEntity> query = dbSet;

    if (filter != null)
    {
      query = query.Where(filter);
    }

    foreach (var includeProperty in includeProperties.Split
        (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
    {
      query = query.Include(includeProperty);
    }

    if (orderBy != null)
    {
      return orderBy(query).ToList();
    }
    else
    {
      return query.ToList();
    }
  }

  /// <summary>
  /// Gets record by id
  /// </summary>
  /// <param name="id">Id to read</param>
  /// <returns>Matched record</returns>
  public async virtual Task<TEntity> GetByIdAsync(uint id)
  {
    return await dbSet.FindAsync(id);
  }

  /// <summary>
  /// Adds a record to the database
  /// </summary>
  /// <param name="phys">Record to add</param>
  /// <returns>Record added</returns>
  public virtual async Task<TEntity> InsertAsync(
    TEntity phys)
  {
    await dbSet.AddAsync(phys);
    return phys;
  }

  /// <summary>
  /// Updates a record in the database
  /// </summary>
  /// <param name="phys">Record to update</param>
  /// <returns>Record updated</returns>
  public TEntity Update(
    TEntity phys)
  {
    dbSet.Update(phys);
    return phys;
  }
}
