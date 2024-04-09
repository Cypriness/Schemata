using Schemata.Mapping.Foundation;
using Schemata.Mapping.Foundation.Features;
using Schemata.Mapping.Mapster;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class SchemataMappingBuilderExtensions
{
    public static SchemataMappingBuilder UseMapster(this SchemataMappingBuilder builder) {
        builder.Builder.AddFeature<SchemataMappingFeature<SimpleMapper>>();

        return builder;
    }
}
