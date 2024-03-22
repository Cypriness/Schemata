using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Schemata.Core.Features;

public class SchemataPageFeature : FeatureBase
{
    public override int Priority => 210_000_000;

    public override void ConfigureServices(
        IServiceCollection  services,
        Configurators       configurators,
        IConfiguration      configuration,
        IWebHostEnvironment environment) {
        var configure = configurators.Get<RazorPagesOptions>();
        services.AddRazorPages(configure);
    }

    public override void ConfigureEndpoint(
        IEndpointRouteBuilder endpoint,
        IConfiguration        configuration,
        IWebHostEnvironment   environment) {
        endpoint.MapRazorPages();
    }
}
