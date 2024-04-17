using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Schemata.Abstractions;
using Schemata.Abstractions.Advices;
using Schemata.Abstractions.Entities;

namespace Schemata.Entity.Repository.Advices;

public class AdviceAddCanonicalName
{
    protected static readonly Regex ResourceNameRegex = new(@"\{(?<name>\w+)\}");
}

public sealed class AdviceAddCanonicalName<TEntity> : AdviceAddCanonicalName, IRepositoryAddAsyncAdvice<TEntity>
    where TEntity : class
{
    #region IRepositoryAddAsyncAdvice<TEntity> Members

    public int Order => 300_000_000;

    public int Priority => Order;

    public Task<bool> AdviseAsync(
        AdviceContext        ctx,
        IRepository<TEntity> repository,
        TEntity              entity,
        CancellationToken    ct) {
        if (entity is not ICanonicalName named) {
            return Task.FromResult(true);
        }

        var type = entity.GetType();

        var attribute = type.GetCustomAttribute<CanonicalNameAttribute>(false);
        if (attribute is null) {
            return Task.FromResult(true);
        }

        var current = type.GetCustomAttribute<DisplayNameAttribute>(false)?.DisplayName.Singularize()
                   ?? type.GetCustomAttribute<TableAttribute>(false)?.Name.Singularize() ?? type.Name;

        var properties = AppDomainTypeCache.GetProperties(type);

        var name = ResourceNameRegex.Replace(attribute.ResourceName, m => {
            var matched = m.Groups["name"].Value.Pascalize();
            if (string.Equals(current, matched)) {
                matched = string.Empty;
            }

            if (!properties.TryGetValue($"{matched}Name", out var property)) {
                throw new MissingFieldException(type.Name, $"{matched}Name");
            }

            return property.GetValue(entity)?.ToString() ?? string.Empty;
        });

        named.CanonicalName = name;

        return Task.FromResult(true);
    }

    #endregion
}
