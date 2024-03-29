using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Schemata.Core;

public class SchemataOptions
{
    private readonly Dictionary<string, object> _options = new();

    public ILoggerFactory Logging { get; set; } = LoggerFactory.Create(_ => { });

    public ILogger<SchemataBuilder> Logger => CreateLogger<SchemataBuilder>();

    public ILogger<T> CreateLogger<T>() {
        return Logging.CreateLogger<T>();
    }

    public object? CreateLogger(Type type) {
        var logger  = typeof(Logger<>);
        var generic = logger.MakeGenericType(type);

        return Activator.CreateInstance(generic, Logging);
    }

    public TOptions? Get<TOptions>(string name)
        where TOptions : class {
        if (!_options.TryGetValue(name, out var value)) {
            return null;
        }

        return value is TOptions options ? options : null;
    }

    public void Set<TOptions>(string name, TOptions? options)
        where TOptions : class {
        if (options is null) {
            _options.Remove(name);
            return;
        }

        _options[name] = options;
    }
}
