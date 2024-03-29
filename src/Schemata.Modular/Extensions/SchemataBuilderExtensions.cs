using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Schemata.Core;
using Schemata.Core.Features;
using Schemata.Modular;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public static class SchemataBuilderExtensions
{
    public static SchemataBuilder UseModular(this SchemataBuilder builder) {
        return UseModular<DefaultModulesRunner>(builder);
    }

    public static SchemataBuilder UseModular<TRunner>(this SchemataBuilder builder)
        where TRunner : class, IModulesRunner {
        return UseModular(builder, typeof(TRunner), new[] { typeof(DefaultModulesProvider) });
    }

    public static SchemataBuilder UseModular<TRunner>(this SchemataBuilder builder, IEnumerable<Type> providers)
        where TRunner : class, IModulesRunner {
        return UseModular(builder, typeof(TRunner), providers);
    }

    public static SchemataBuilder UseModular(this SchemataBuilder builder, Type runner, IEnumerable<Type> providers) {
        builder.ConfigureServices(services => {
            var modules = providers
                         .Select(p => Utilities.CreateInstance<IModulesProvider>(p, builder.Options.CreateLogger(p),
                              builder.Configuration, builder.Environment))
                         .OfType<IModulesProvider>()
                         .SelectMany(p => p.GetModules())
                         .ToList();
            builder.Options.SetModules(modules);

            if (builder.Options.HasFeature<SchemataControllersFeature>()) {
                var part = new ModularApplicationPart();

                services.AddSingleton(part);
                services.AddSingleton<IActionDescriptorChangeProvider>(part);

                services.AddMvcCore()
                        .ConfigureApplicationPartManager(manager => { manager.ApplicationParts.Add(part); });
            }

            if (services.All(s => s.ServiceType != typeof(IModulesRunner))) {
                // To avoid accessing the builder.Configure() method and builder.ConfigureServices() method after building the service provider,
                // we create a runner here instead of in the delegate.

                var run = Utilities.CreateInstance<IModulesRunner>(runner, builder.Options.CreateLogger(runner),
                    builder.Options)!;
                run.ConfigureServices(services, builder.Configuration, builder.Environment);
                services.TryAddSingleton<IModulesRunner>(_ => run);
            }

            services.AddTransient<IStartupFilter, ModularStartup>(sp => ModularStartup.Create(
                builder.Configuration, // and builder.ConfigureServices() method
                builder.Environment,   // after building the service provider.
                sp                     //
            ));
        });

        return builder;
    }
}
