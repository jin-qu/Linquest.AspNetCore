using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Linquest.AspNetCore.Tests.Fixture {
    using Interface;
    using Model;

    public class Startup : StartupBase {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services) {
#if NETCOREAPP2_1
            services.AddMvc();
            services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);
#elif NETCOREAPP3_0
            services.AddControllers();
#endif
            services.AddSingleton<IContentHandler<Product>, ProductHandler>();
        }

        public override void Configure(IApplicationBuilder app) {
#if NETCOREAPP2_1
            app.UseMvc();
#elif NETCOREAPP3_0
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
                endpoints.MapControllers();
            });
#endif
        }
    }
}