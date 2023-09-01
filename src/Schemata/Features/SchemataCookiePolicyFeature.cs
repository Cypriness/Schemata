using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Schemata.Features;

public class SchemataCookiePolicyFeature : FeatureBase
{
    public override int Priority => 140_000_000;

    public override void ConfigureServices(
        IServiceCollection  services,
        Configurators       configurators,
        IConfiguration      configuration,
        IWebHostEnvironment environment) {
        var configure = configurators.Get<CookiePolicyOptions>();
        services.AddCookiePolicy(configure);
    }

    public override void Configure(
        IApplicationBuilder app,
        IConfiguration      configuration,
        IWebHostEnvironment environment) {
        app.UseCookiePolicy();
    }
}
