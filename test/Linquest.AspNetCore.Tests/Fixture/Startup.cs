using System;
using System.Collections.Generic;
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
            services.AddMvc();
            services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);

            services.AddSingleton<IContentHandler<Product>, ProductHandler>();
        }

        public override void Configure(IApplicationBuilder app) => app.UseMvc();
    }
}
