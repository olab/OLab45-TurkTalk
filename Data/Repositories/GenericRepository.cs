using Dawn;
using Microsoft.EntityFrameworkCore;
using OLab.Common.Interfaces;
using System.Linq.Expressions;

namespace OLab.TurkTalk.Data.Repositories;
public abstract class GenericRepository<TEntity> where TEntity : class
{
  public IOLabLogger Logger { get; }
  public TTalkDBContext DbContext { get; }
  public DbSet<TEntity> DbSet;
  protected readonly DatabaseUnitOfWork DbUnitOfWork;

  public GenericRepository(
    IOLabLogger logger,
    TTalkDBContext dbContext)
  {
    Guard.Argument(logger).NotNull(nameof(logger));
    Guard.Argument(dbContext).NotNull(nameof(dbContext));

    Logger = logger;
    DbContext = dbContext;
    DbSet = DbContext.Set<TEntity>();
  }

  protected GenericRepository(DatabaseUnitOfWork databaseUnitOfWork) : 
    this(databaseUnitOfWork.Logger, databaseUnitOfWork.DbContextTT)
  {
    Guard.Argument(databaseUnitOfWork, nameof(databaseUnitOfWork)).NotNull();

    DbUnitOfWork = databaseUnitOfWork;
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
    IQueryable<TEntity> query = DbSet;

    if (filter != null)
    {
      query = query.Where(filter);
    }

    foreach (var includeProperty in includeProperties.Split
        (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
    {
      query = query.Include(includeProperty.Trim());
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
  public virtual async Task<TEntity> GetByIdAsync(uint id)
  {
    Guard.Argument(id, nameof(id)).Positive();

    return await DbSet.FindAsync(id);
  }

  /// <summary>
  /// Removes record to the database
  /// </summary>
  /// <param name="phys">Record to add</param>
  /// <returns>Record added</returns>
  public virtual void Remove(
    TEntity phys)
  {
    Guard.Argument(phys, nameof(phys)).NotNull();
    DbSet.Remove(phys);
  }

  /// <summary>
  /// Adds a record to the database
  /// </summary>
  /// <param name="phys">Record to add</param>
  /// <returns>Record added</returns>
  public virtual async Task<TEntity> InsertAsync(
    TEntity phys)
  {
    Guard.Argument(phys, nameof(phys)).NotNull();

    await DbSet.AddAsync(phys);
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
    Guard.Argument(phys, nameof(phys)).NotNull();

    DbSet.Update(phys);
    return phys;
  }
}
