using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Linquest.AspNetCore.Tests.Fixture {

    public class Startup: StartupBase {

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services) => services.AddMvc();

        public override void Configure(IApplicationBuilder app) => app.UseMvc();
    }
}
