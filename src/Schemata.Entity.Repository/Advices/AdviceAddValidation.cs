using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Schemata.Abstractions;
using Schemata.Abstractions.Advices;
using Schemata.Abstractions.Entities;
using Schemata.Abstractions.Exceptions;

namespace Schemata.Entity.Repository.Advices;

public sealed class AdviceAddValidation<TEntity>(IServiceProvider services) : IRepositoryAddAsyncAdvice<TEntity>
    where TEntity : class
{
    #region IRepositoryAddAsyncAdvice<TEntity> Members

    public int Order => SchemataConstants.Orders.Max;

    public int Priority => Order;

    public async Task<bool> AdviseAsync(
        AdviceContext        ctx,
        IRepository<TEntity> repository,
        TEntity              entity,
        CancellationToken    ct) {
        if (ctx.Has<SuppressAddValidation>()) {
            return true;
        }

        var errors = new List<KeyValuePair<string, string>>();
        var pass = await Advices<IValidationAsyncAdvice<TEntity>>.AdviseAsync(services, ctx, Operations.Create, entity, errors, ct);
        if (pass) {
            return true;
        }

        throw new ValidationException(errors);
    }

    #endregion
}
