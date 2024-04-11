using System;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Options;
using Schemata.Mapping.Skeleton;

namespace Schemata.Mapping.Mapster;

public sealed class SimpleMapper : ISimpleMapper
{
    private readonly Mapper _mapper;

    public SimpleMapper(IOptions<SchemataMappingOptions> options) {
        var config = TypeAdapterConfig.GlobalSettings;

        MapsterConfigurator.Configure(config, options.Value);

        _mapper = new(config);
    }

    #region ISimpleMapper Members

    public T? Map<T>(object source) {
        return _mapper.Map<T>(source);
    }

    public T? Map<T>(object source, Type sourceType) {
        return (T?)_mapper.Map(source, sourceType, typeof(T));
    }

    public TDestination? Map<TSource, TDestination>(TSource source) {
        return _mapper.Map<TSource, TDestination>(source);
    }

    public void Map<TSource, TDestination>(TSource source, TDestination destination) {
        _mapper.Map(source, destination);
    }

    public object? Map(object source, Type destinationType) {
        return _mapper.Map(source, source.GetType(), destinationType);
    }

    public object? Map(object source, Type sourceType, Type destinationType) {
        return _mapper.Map(source, sourceType, destinationType);
    }

    public void Map(
        object source,
        object destination,
        Type   sourceType,
        Type   destinationType) {
        _mapper.Map(source, destination, sourceType, destinationType);
    }

    #endregion
}
