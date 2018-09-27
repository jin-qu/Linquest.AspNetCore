using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public async Task ShouldGetSortedOrders() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();

                var url1 = $"api/Test/Orders?$orderBy={WebUtility.UrlEncode("o => o.Price")}&$thenByDescending={WebUtility.UrlEncode("o => o.Id")}";
                var response1 = await client.GetStringAsync(url1);
                var orders1 = JsonConvert.DeserializeObject<List<Order>>(response1);

                Assert.Equal(5, orders1.Count);
                Assert.Equal(4, orders1[0].Id);
                Assert.Equal("Ord3", orders1[4].No);

                var url2 = $"api/Test/Orders?$orderByDescending={WebUtility.UrlEncode("o => o.Price")}&$thenBy={WebUtility.UrlEncode("o => o.Id")}";
                var response2 = await client.GetStringAsync(url2);
                var orders2 = JsonConvert.DeserializeObject<List<Order>>(response2);

                Assert.Equal(5, orders2.Count);
                Assert.Equal(3, orders2[0].Id);
                Assert.Equal("Ord4", orders2[4].No);
            }
        }

        [Fact]
        public async Task ShouldProjectOrders() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$select={WebUtility.UrlEncode("o => new { o.Id, o.No }")}";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<JObject>>(response);

                Assert.Equal(5, orders.Count);
                Assert.Equal(2, orders[0].Properties().Count());
                Assert.Equal(1, orders[0].Property("id").Value);
                Assert.Equal("Ord5", orders[4].Property("no").Value);
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
        public async Task ShouldSelectManyDetails() {
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
        public async Task ShouldExecuteSum() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$sum={WebUtility.UrlEncode("o => o.Price")}";
                var response = await client.GetStringAsync(url);

                Assert.Equal(3632f, float.Parse(response));
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
        public async Task ShouldGetFirstOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$first={WebUtility.UrlEncode("o => o.Id > 3")}";
                var response = await client.GetStringAsync(url);
                var order = JsonConvert.DeserializeObject<Order>(response);

                Assert.Equal(4, order.Id);
            }
        }

        [Fact]
        public async Task ShouldGetNullForNonExistingFirstOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$firstOrDefault={WebUtility.UrlEncode("o => o.Id > 5")}";
                var response = await client.GetStringAsync(url);

                Assert.Empty(response);
            }
        }

        [Fact]
        public async Task ShouldGetSingleOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$single={WebUtility.UrlEncode("o => o.Id > 4")}";
                var response = await client.GetStringAsync(url);
                var order = JsonConvert.DeserializeObject<Order>(response);

                Assert.Equal(5, order.Id);
            }
        }

        [Fact]
        public async Task ShouldGetNullForNonExistingSingleOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$singleOrDefault={WebUtility.UrlEncode("o => o.Id > 5")}";
                var response = await client.GetStringAsync(url);

                Assert.Empty(response);
            }
        }

        [Fact]
        public async Task ShouldGetLastOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$last={WebUtility.UrlEncode("o => o.Id > 2")}";
                var response = await client.GetStringAsync(url);
                var order = JsonConvert.DeserializeObject<Order>(response);

                Assert.Equal(5, order.Id);
            }
        }

        [Fact]
        public async Task ShouldGetNullForNonExistingLastOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$lastOrDefault={WebUtility.UrlEncode("o => o.Id > 5")}";
                var response = await client.GetStringAsync(url);

                Assert.Empty(response);
            }
        }

        [Fact]
        public async Task ShouldGetElementAtIndex() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = "api/Test/Orders?$elementAt=3";
                var response = await client.GetStringAsync(url);
                var order = JsonConvert.DeserializeObject<Order>(response);

                Assert.Equal(4, order.Id);
            }
        }

        [Fact]
        public async Task ShouldGetNullForNonExistingElementAtIndex() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$elementAtOrDefault=5";
                var response = await client.GetStringAsync(url);

                Assert.Empty(response);
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

        [Fact]
        public async Task ShouldThrowForUnsupportedQuery() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                await Assert.ThrowsAsync<Exception>(() => client.GetStringAsync("api/Test/Orders?$intersect"));
            }
        }

        private TestServer CreateTestServer() {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            return new TestServer(builder);
        }
    }
}
