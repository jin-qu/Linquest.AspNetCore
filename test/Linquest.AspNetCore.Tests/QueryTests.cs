using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Xunit;

namespace Linquest.AspNetCore.Tests {
    using System;
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
        public async Task ShouldReturnNull() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var value = await client.GetStringAsync("api/Test/NullValue");
                Assert.Empty(value);
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
        public async Task ShouldGetOrdersIdGreaterThan2() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$skipWhile={WebUtility.UrlEncode("o => o.Id < 3")}";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(3, orders.Count);
                Assert.Equal(3, orders[0].Id);
                Assert.Equal("Ord4", orders[1].No);
            }
        }

        [Fact]
        public async Task ShouldGetOrdersIdLessThan3() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$takeWhile={WebUtility.UrlEncode("o => o.Id < 3")}";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(2, orders.Count);
                Assert.Equal(1, orders[0].Id);
                Assert.Equal("Ord2", orders[1].No);
            }
        }

        [Fact]
        public async Task ShouldGetPageData() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = "api/Test/Orders?$inlineCount=allpages&$skip=2&$take=2";
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

        [Fact]
        public async Task ShouldExecuteAll() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$all={WebUtility.UrlEncode("o => o.Id > 3")}";
                var response = await client.GetStringAsync(url);

                Assert.Equal("false", response);
            }
        }

        [Fact]
        public async Task ShouldExecuteAny() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$any={WebUtility.UrlEncode("o => o.Id < 3")}";
                var response = await client.GetStringAsync(url);

                Assert.Equal("true", response);
            }
        }

        [Fact]
        public async Task ShouldExecuteAvg() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$avg={WebUtility.UrlEncode("o => o.Id")}";
                var response = await client.GetStringAsync(url);

                Assert.Equal(3, float.Parse(response));
            }
        }

        [Fact]
        public async Task ShouldExecuteMax() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$max={WebUtility.UrlEncode("o => o.Price")}";
                var response = await client.GetStringAsync(url);

                Assert.Equal(1125f, float.Parse(response));
            }
        }

        [Fact]
        public async Task ShouldExecuteMin() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$min={WebUtility.UrlEncode("o => o.Price")}";
                var response = await client.GetStringAsync(url);

                Assert.Equal(231.58f, float.Parse(response));
            }
        }

        [Fact]
        public async Task ShouldExecuteCount() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$count={WebUtility.UrlEncode("o => o.Id > 3")}";
                var response = await client.GetStringAsync(url);

                Assert.Equal("2", response);
            }
        }

        [Fact]
        public async Task ShouldReverseOrders() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$reverse";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(5, orders.Count);
                Assert.Equal(5, orders[0].Id);
                Assert.Equal("Ord1", orders[4].No);
            }
        }

        [Fact]
        public async Task ShouldThrowWhenMaxCountExceeded() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                await Assert.ThrowsAsync<Exception>(() => client.GetStringAsync("api/Test/LimitedOrders"));
                await Assert.ThrowsAsync<Exception>(() => client.GetStringAsync("api/Test/LimitedOrders?$take=4"));
            }
        }

        private TestServer CreateTestServer() {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            return new TestServer(builder);
        }
    }
}
