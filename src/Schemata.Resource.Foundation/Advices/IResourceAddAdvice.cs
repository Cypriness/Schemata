using Microsoft.AspNetCore.Http;
using Schemata.Abstractions;
using Schemata.Abstractions.Entities;

namespace Schemata.Resource.Foundation.Advices;

public interface IResourceAddAdvice<TEntity, TRequest> : IAdvice<TRequest, HttpContext>
    where TEntity : class, IIdentifier
    where TRequest : class, IIdentifier;
