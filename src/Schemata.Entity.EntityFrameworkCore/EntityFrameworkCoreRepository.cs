using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Schemata.Entity.Repository;
using Schemata.Entity.Repository.Advices;

namespace Schemata.Entity.EntityFrameworkCore;

public class EntityFrameworkCoreRepository<TContext, TEntity> : RepositoryBase<TEntity>
    where TContext : DbContext
    where TEntity : class
{
    public EntityFrameworkCoreRepository(TContext context, IServiceProvider provider) {
        Context  = context;
        Provider = provider;
    }

    protected TContext         Context  { get; }
    protected IServiceProvider Provider { get; }

    public override async IAsyncEnumerable<TEntity> ListAsync(
        Expression<Func<TEntity, bool>>?           predicate,
        [EnumeratorCancellation] CancellationToken ct = default) {
        var enumerable = BuildQuery(predicate).AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var entity in enumerable) yield return entity;
    }

    public override async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        return await BuildQuery(predicate).FirstOrDefaultAsync(ct);
    }

    public override async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        return await BuildQuery(predicate).SingleOrDefaultAsync(ct);
    }

    public override async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        return await BuildQuery(predicate).AnyAsync(ct);
    }

    public override async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        return await BuildQuery(predicate).CountAsync(ct);
    }

    public override async Task<long> LongCountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken                ct = default) {
        return await BuildQuery(predicate).LongCountAsync(ct);
    }

    public override async Task AddAsync(TEntity entity, CancellationToken ct = default) {
        await Advices<IRepositoryAddAsyncAdvice<TEntity>>.AdviseAsync(Provider, entity, ct);

        await Context.AddAsync(entity, ct);
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default) {
        await Advices<IRepositoryUpdateAsyncAdvice<TEntity>>.AdviseAsync(Provider, entity, ct);

        Context.Entry(entity).State = EntityState.Detached;
        Context.Update(entity);
    }

    public override async Task RemoveAsync(TEntity entity, CancellationToken ct = default) {
        await Advices<IRepositoryRemoveAsyncAdvice<TEntity>>.AdviseAsync(Provider, entity, ct);

        Context.Remove(entity);
    }

    public override async Task<int> CommitAsync(CancellationToken ct = default) {
        return await Context.SaveChangesAsync(ct);
    }

    private IQueryable<TEntity> BuildQuery(Expression<Func<TEntity, bool>>? predicate) {
        var table = Context.Set<TEntity>();

        return predicate != null ? table.Where(predicate) : table;
    }
}
