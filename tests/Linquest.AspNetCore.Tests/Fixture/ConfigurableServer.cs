using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Linquest.AspNetCore.Tests.Fixture {

    public class ConfigurableServer : TestServer {

        public ConfigurableServer(Action<IServiceCollection> configureAction = null) : base(CreateBuilder(configureAction)) {}

        private static IWebHostBuilder CreateBuilder(Action<IServiceCollection> configureAction) {
            if (configureAction == null) {
                configureAction = sc => {};
            }

            return new WebHostBuilder()
                .ConfigureServices(sc => sc.AddSingleton<Action<IServiceCollection>>(configureAction))
                .UseStartup<Startup>();
        }
    }
}
