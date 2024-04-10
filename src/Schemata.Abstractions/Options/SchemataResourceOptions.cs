using System;
using System.Collections.Generic;
using Schemata.Abstractions.Resource;

namespace Schemata.Abstractions.Options;

public sealed class SchemataResourceOptions
{
    public Dictionary<Type, ResourceAttribute> Resources { get; } = [];
}
