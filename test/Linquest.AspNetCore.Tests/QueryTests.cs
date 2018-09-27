using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public async Task ShouldGetFilteredOrdersFromNativeController() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test2/Orders?$where={WebUtility.UrlEncode("o => o.Id > 3")}";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(2, orders.Count);
                Assert.Equal(4, orders[0].Id);
                Assert.Equal("Ord5", orders[1].No);
            }
        }

        [Fact]
        public async Task ShouldNotFilterEnumerableOrders() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/EnumerableOrders?$where={WebUtility.UrlEncode("o => o.Id > 3")}";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(5, orders.Count);
            }
        }

        [Fact]
        public async Task ShouldNotFilterNonLinquestOrders() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/NonLinquestOrders?$where={WebUtility.UrlEncode("o => o.Id > 3")}";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(5, orders.Count);
            }
        }

        [Fact]
        public async Task ShouldSkipJsonResult() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/JsonOrders?$where={WebUtility.UrlEncode("o => o.Id > 3")}";
                var response = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(5, orders.Count);
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

                var url1 = "api/Test/Orders?$inlineCount=allpages&$skip=2&$take=2";
                var response1 = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url1));
                var content1 = await response1.Content.ReadAsStringAsync();
                var orders1 = JsonConvert.DeserializeObject<List<Order>>(content1);

                Assert.Equal(2, orders1.Count);
                Assert.Equal(3, orders1[0].Id);
                Assert.Equal("Ord4", orders1[1].No);
                var inlineCountHeader = response1.Headers.FirstOrDefault(h => h.Key == "X-InlineCount");
                Assert.Equal("5", inlineCountHeader.Value.FirstOrDefault());

                var url2 = "api/Test/Orders?$inlineCount=false&$skip=2&$take=2";
                var response2 = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url2));
                var content2 = await response2.Content.ReadAsStringAsync();
                var orders2 = JsonConvert.DeserializeObject<List<Order>>(content2);

                Assert.Equal(2, orders2.Count);
                Assert.Equal(3, orders2[0].Id);
                Assert.Equal("Ord4", orders2[1].No);
                inlineCountHeader = response2.Headers.FirstOrDefault(h => h.Key == "X-InlineCount");
                Assert.Equal(default(KeyValuePair<string, IEnumerable<string>>), inlineCountHeader);

                var url3 = "api/Test/Orders?$inlineCount&$take=2";
                var response3 = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url3));
                var content3 = await response3.Content.ReadAsStringAsync();
                var orders3 = JsonConvert.DeserializeObject<List<Order>>(content3);

                Assert.Equal(2, orders3.Count);
                Assert.Equal(1, orders3[0].Id);
                Assert.Equal("Ord2", orders3[1].No);
                inlineCountHeader = response3.Headers.FirstOrDefault(h => h.Key == "X-InlineCount");
                Assert.Equal("5", inlineCountHeader.Value.FirstOrDefault());
            }
        }

        [Fact]
        public async Task ShouldGroupByCustomer() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();

                var url1 = $"api/Test/Orders?$groupBy={WebUtility.UrlEncode("o => o.Price")};{WebUtility.UrlEncode("(k, g) => g.Count()")}";
                var response1 = await client.GetStringAsync(url1);
                var counts = JsonConvert.DeserializeObject<List<int>>(response1);

                Assert.Equal(new[] { 1, 1, 2, 1 }, counts);

                var url2 = $"api/Test/Orders?$groupBy={WebUtility.UrlEncode("o => o.Price")}";
                var response2 = await client.GetStringAsync(url2);
                var groups = JsonConvert.DeserializeObject<List<List<Order>>>(response2);

                Assert.Single(groups[0]);
                Assert.Single(groups[1]);
                Assert.Equal(2, groups[2].Count);
                Assert.Single(groups[3]);

                var url3 = $"api/Test/Orders?$groupBy=a;b;c";

                await Assert.ThrowsAsync<Exception>(() => client.GetStringAsync(url3));
            }
        }

        [Fact]
        public async Task ShouldGetDistinctPriceCount() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$select={WebUtility.UrlEncode("o => o.Price")}&$distinct&$count";
                var response = await client.GetStringAsync(url);

                Assert.Equal(4, Convert.ToInt32(response));
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
        public async Task ShouldExecuteAggregate() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();

                var url1 = $"api/Test/Orders?$select=Price&$aggregate={WebUtility.UrlEncode("(p1, p2) => p1 + p2")}";
                var response1 = await client.GetStringAsync(url1);

                Assert.Equal(3632f, float.Parse(response1));

                var x = Consts.Orders.Select(o => o.Price).Aggregate(1f, (p1, p2) => p1 + p2);
                var url2 = $"api/Test/Orders?$select=Price&$aggregate={WebUtility.UrlEncode("(p1, p2) => p1 + p2")};10";
                var response2 = await client.GetStringAsync(url2);

                Assert.Equal(3642f, float.Parse(response2), 1);

                await Assert.ThrowsAsync<Exception>(() => client.GetStringAsync("api/Test/Orders?$select=Price&$aggregate=a;b;c"));
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
        public async Task ShouldUseCustomHandler() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/GetAProduct";
                var response = await client.GetStringAsync(url);

                Assert.Equal("42", response);
            }
        }

        [Fact]
        public async Task ShouldThrowWhenMaxCountExceeded() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();

                var response = await client.GetStringAsync("api/Test/LimitedOrders?$take=3");
                var orders = JsonConvert.DeserializeObject<List<Order>>(response);

                Assert.Equal(3, orders.Count);
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

        [Fact]
        public void ShouldCheckArguments() {
            var handler = new QueryableHandler();
            var orders = Consts.Orders.AsQueryable();

            Assert.Throws<ArgumentNullException>(() => handler.HandleContent(null, null));
            Assert.Throws<ArgumentNullException>(() => handler.HandleContent(orders, null));

            var handled = handler.HandleContent((object)orders, new ActionContext(null, orders, null, null));

            Assert.Equal(orders.ToList(), handled.Result);
            Assert.NotNull(handled.Context);
        }

        private TestServer CreateTestServer() {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>();
            return new TestServer(builder);
        }
    }
}
