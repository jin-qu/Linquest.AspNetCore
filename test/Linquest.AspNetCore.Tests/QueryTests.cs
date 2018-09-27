using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace Linquest.AspNetCore.Tests {
    using Fixture;
    using Fixture.Model;

    public class QueryTests {

        [Fact]
        public async Task ShouldAccessController() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var value = await client.GetStringAsync("api/Test");
                Assert.Equal("Hello World!", value);
            }
        }

        [Fact]
        public async Task ShouldGetFilteredOrders() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$where={WebUtility.UrlEncode("o => o.Id > 3")}";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(2, orders.Count);
                Assert.Equal(4, orders[0].Id);
                Assert.Equal("Ord5", orders[1].No);
            }
        }

        [Fact]
        public async Task ShouldGetPageData() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = "api/Test/Orders?$skip=2&$take=2";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(2, orders.Count);
                Assert.Equal(3, orders[0].Id);
                Assert.Equal("Ord4", orders[1].No);
            }
        }

        [Fact]
        public async Task ShouldSelectDetails() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$selectMany={WebUtility.UrlEncode("o => o.OrderDetails")}";
                var response = await client.GetStringAsync(url);
                var details = JsonConvert.DeserializeObject<List<OrderDetail>>(response);

                Assert.Equal(16, details.Count);
                Assert.Equal(36, details[4].Count);
                Assert.Equal("Prd6", details[12].Product);
            }
        }

        private TestServer CreateTestServer() {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            return new TestServer(builder);
        }
    }
}
