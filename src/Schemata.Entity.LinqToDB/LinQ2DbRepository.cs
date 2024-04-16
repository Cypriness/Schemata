using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Humanizer;
using LinqToDB;
using LinqToDB.Data;
using Schemata.Entity.Repository;
using Schemata.Entity.Repository.Advices;

namespace Schemata.Entity.LinqToDB;

public class LinQ2DbRepository<TContext, TEntity> : RepositoryBase<TEntity>
    where TContext : DataConnection
    where TEntity : class
{
    private ITable<TEntity>? _table;

    public LinQ2DbRepository(IServiceProvider sp, TContext context) {
        ServiceProvider = sp;
        Context         = context;

        var entity = typeof(TEntity);
        TableName = entity.GetCustomAttribute<TableAttribute>(false)?.Name ?? entity.Name.Pluralize();
    }

    protected virtual TContext Context { get; }

    protected virtual ITable<TEntity> Table => _table ??= Context.GetTable<TEntity>().TableName(TableName);

    protected virtual IServiceProvider ServiceProvider { get; }

    protected virtual DataConnectionTransaction? Transaction { get; set; }

    protected virtual int RowsAffected { get; set; }

    public virtual string TableName { get; }

    public override IAsyncEnumerable<TEntity> AsAsyncEnumerable() {
        return Table.AsAsyncEnumerable();
    }

    public override IQueryable<TEntity> AsQueryable() {
        return Table.AsQueryable();
    }

    public override async IAsyncEnumerable<TResult> ListAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>>? predicate,
        [EnumeratorCancellation] CancellationToken      ct = default) {
        var query      = await BuildQueryAsync(predicate, ct);
        var enumerable = query.AsAsyncEnumerable().WithCancellation(ct);

        await foreach (var entity in enumerable) {
            ct.ThrowIfCancellationRequested();
            yield return entity;
        }
    }

    public override IAsyncEnumerable<TResult> SearchAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>>? predicate,
        CancellationToken                               ct = default) {
        throw new NotImplementedException();
    }

    public override async ValueTask<TResult?> FirstOrDefaultAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>>? predicate,
        CancellationToken                               ct = default)
        where TResult : default {
        var query = await BuildQueryAsync(predicate, ct);
        return await query.FirstOrDefaultAsync(ct);
    }

    public override async ValueTask<TResult?> SingleOrDefaultAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>>? predicate,
        CancellationToken                               ct = default)
        where TResult : default {
        var query = await BuildQueryAsync(predicate, ct);
        return await query.SingleOrDefaultAsync(ct);
    }

    public override async ValueTask<bool> AnyAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>>? predicate,
        CancellationToken                               ct = default) {
        var query = await BuildQueryAsync(predicate, ct);
        return await query.AnyAsync(ct);
    }

    public override async ValueTask<int> CountAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>>? predicate,
        CancellationToken                               ct = default) {
        var query = await BuildQueryAsync(predicate, ct);
        return await query.CountAsync(ct);
    }

    public override async ValueTask<long> LongCountAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>>? predicate,
        CancellationToken                               ct = default) {
        var query = await BuildQueryAsync(predicate, ct);
        return await query.LongCountAsync(ct);
    }

    public override async Task AddAsync(TEntity entity, CancellationToken ct = default) {
        await BeginTransactionAsync(ct);

        var next = await Advices<IRepositoryAddAsyncAdvice<TEntity>>.AdviseAsync(ServiceProvider, this, entity, ct);
        if (!next) {
            return;
        }

        RowsAffected += await Context.InsertAsync(entity, TableName, token: ct);
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default) {
        await BeginTransactionAsync(ct);

        var next = await Advices<IRepositoryUpdateAsyncAdvice<TEntity>>.AdviseAsync(ServiceProvider, this, entity, ct);
        if (!next) {
            return;
        }

        RowsAffected += await Context.UpdateAsync(entity, TableName, token: ct);
    }

    public override async Task RemoveAsync(TEntity entity, CancellationToken ct = default) {
        await BeginTransactionAsync(ct);

        var next = await Advices<IRepositoryRemoveAsyncAdvice<TEntity>>.AdviseAsync(ServiceProvider, this, entity, ct);
        if (!next) {
            return;
        }

        RowsAffected += await Context.DeleteAsync(entity, TableName, token: ct);
    }

    public override async ValueTask<int> CommitAsync(CancellationToken ct = default) {
        ct.ThrowIfCancellationRequested();

        if (Transaction is null) {
            return 0;
        }

        try {
            await Transaction.CommitAsync(ct);
        } catch (Exception ex) {
            await Transaction.RollbackAsync(ct);

            throw new TransactionAbortedException(ex.Message, ex);
        }

        var rows = RowsAffected;

        Transaction  = null;
        RowsAffected = 0;

        return rows;
    }

    private async Task<IQueryable<TResult>> BuildQueryAsync<TResult>(
        Func<IQueryable<TEntity>, IQueryable<TResult>>? predicate,
        CancellationToken                               ct) {
        ct.ThrowIfCancellationRequested();

        var table = AsQueryable();

        var query = new QueryContainer<TEntity>(table);

        await Advices<IRepositoryQueryAsyncAdvice<TEntity>>.AdviseAsync(ServiceProvider, this, query, ct);

        return BuildQuery(query.Query, predicate);
    }

    private async Task BeginTransactionAsync(CancellationToken ct) {
        ct.ThrowIfCancellationRequested();

        if (Transaction is not null) {
            return;
        }

        Transaction = await Context.BeginTransactionAsync(ct);
    }
}
