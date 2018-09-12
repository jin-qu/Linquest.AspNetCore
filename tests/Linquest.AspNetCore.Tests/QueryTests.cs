using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Linquest.AspNetCore.Tests {
    using System.Collections.Generic;
    using System.Net;
    using Fixture;
    using Linquest.AspNetCore.Tests.Fixture.Model;
    using Newtonsoft.Json;

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
                var url = $"api/Test/Orders?$where={WebUtility.UrlEncode("it.Id > 3")}";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(2, orders.Count);
                Assert.Equal(4, orders[0].Id);
                Assert.Equal("Ord5", orders[1].No);
            }
        }

        private TestServer CreateTestServer() {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            return new TestServer(builder);
        }
    }
}
