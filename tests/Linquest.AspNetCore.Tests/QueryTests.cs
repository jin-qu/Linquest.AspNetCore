using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Linquest.AspNetCore.Tests {
    using System.Net;
    using Fixture;

    public class QueryTests {

        [Fact]
        public async Task ShouldAccessController() {
            using(var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var value = await client.GetStringAsync("api/Test");
                Assert.Equal("Hello World!", value);
            }
        }

        [Fact]
        public async Task ShouldGetFilteredOrders() {
            using(var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var where = WebUtility.UrlEncode("it.Id > 3");
                var value = await client.GetStringAsync("api/Test/Orders?$where=" + where);
            }
        }

        private TestServer CreateTestServer() {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            return new TestServer(builder);
        }
    }
}
