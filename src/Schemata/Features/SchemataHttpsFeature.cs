using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Schemata.Features;

public class SchemataHttpsFeature : FeatureBase
{
    public override int Priority => 120_000_000;

    public override void Configure(IApplicationBuilder app, IConfiguration conf, IWebHostEnvironment env) {
        if (env.IsDevelopment()) return;

        app.UseHsts();
        app.UseHttpsRedirection();
    }
}
