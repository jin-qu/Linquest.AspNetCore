using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Linquest.AspNetCore.Tests {
    using Fixture;

    public class QueryTests {

        [Fact]
        public async Task ShouldGetFilteredOrders() {
            using(var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var value = await client.GetStringAsync("api/Test/Orders");
                Assert.Equal("Hello world", value);
            }
        }

        private TestServer CreateTestServer() {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            return new TestServer(builder);
        }
    }
}
