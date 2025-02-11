using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Linquest.AspNetCore.Tests.Fixture;

using Interface;
using Model;

public class Startup : StartupBase {

    public override void ConfigureServices(IServiceCollection services) {
        services.AddControllers();
        services.AddSingleton<IContentHandler<Product>, ProductHandler>();
    }

    public override void Configure(IApplicationBuilder app) {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
                
            endpoints.MapControllers();
        });
    }
}
