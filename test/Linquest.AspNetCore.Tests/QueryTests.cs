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
                var content = await client.GetStringAsync("api/Test/NullValue");
                var dynContent = JObject.Parse(content);

                Assert.Empty(dynContent["d"]);
            }
        }

        [Fact]
        public async Task ShouldGetFilteredOrders() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$where={WebUtility.UrlEncode("o => o.Id > 3")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var orders = dynContent["d"].ToObject<List<Order>>();

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
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var orders = dynContent["d"].ToObject<List<Order>>();

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
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var orders = dynContent["d"].ToObject<List<Order>>();

                Assert.Equal(5, orders.Count);
            }
        }

        [Fact]
        public async Task ShouldNotFilterNonLinquestOrders() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/NonLinquestOrders?$where={WebUtility.UrlEncode("o => o.Id > 3")}";
                var content = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(content);

                Assert.Equal(5, orders.Count);
            }
        }

        [Fact]
        public async Task ShouldSkipJsonResult() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/JsonOrders?$where={WebUtility.UrlEncode("o => o.Id > 3")}";
                var content = await client.GetStringAsync(url);
                var orders = JsonConvert.DeserializeObject<List<Order>>(content);

                Assert.Equal(5, orders.Count);
            }
        }


        [Fact]
        public async Task ShouldGetSortedOrders() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();

                var url1 = $"api/Test/Orders?$orderBy={WebUtility.UrlEncode("o => o.Price")}&$thenByDescending={WebUtility.UrlEncode("o => o.Id")}";
                var content1 = await client.GetStringAsync(url1);
                var dynContent1 = JObject.Parse(content1);
                var orders1 = dynContent1["d"].ToObject<List<Order>>();

                Assert.Equal(5, orders1.Count);
                Assert.Equal(4, orders1[0].Id);
                Assert.Equal("Ord3", orders1[4].No);

                var url2 = $"api/Test/Orders?$orderByDescending={WebUtility.UrlEncode("o => o.Price")}&$thenBy={WebUtility.UrlEncode("o => o.Id")}";
                var content2 = await client.GetStringAsync(url2);
                var dynContent2 = JObject.Parse(content2);
                var orders2 = dynContent2["d"].ToObject<List<Order>>();

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
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var orders = dynContent["d"].ToObject<List<JObject>>();

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
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var orders = dynContent["d"].ToObject<List<Order>>();

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
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var orders = dynContent["d"].ToObject<List<Order>>();

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
                var dynContent1 = JObject.Parse(content1);
                var orders1 = dynContent1["d"].ToObject<List<Order>>();

                Assert.Equal(2, orders1.Count);
                Assert.Equal(3, orders1[0].Id);
                Assert.Equal("Ord4", orders1[1].No);
                Assert.Equal("5", dynContent1["inlineCount"].ToString());

                var url2 = "api/Test/Orders?$inlineCount=false&$skip=2&$take=2";
                var response2 = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url2));
                var content2 = await response2.Content.ReadAsStringAsync();
                var dynContent2 = JObject.Parse(content2);
                var orders2 = dynContent2["d"].ToObject<List<Order>>();

                Assert.Equal(2, orders2.Count);
                Assert.Equal(3, orders2[0].Id);
                Assert.Equal("Ord4", orders2[1].No);
                Assert.False(dynContent2.TryGetValue("inlineCount", StringComparison.CurrentCulture, out JToken _));

                var url3 = "api/Test/Orders?$inlineCount&$take=2";
                var response3 = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url3));
                var content3 = await response3.Content.ReadAsStringAsync();
                var dynContent3 = JObject.Parse(content3);
                var orders3 = dynContent3["d"].ToObject<List<Order>>();

                Assert.Equal(2, orders3.Count);
                Assert.Equal(1, orders3[0].Id);
                Assert.Equal("Ord2", orders3[1].No);
                Assert.Equal("5", dynContent3["inlineCount"].ToString());
            }
        }

        [Fact]
        public async Task ShouldGroupByCustomer() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();

                var url1 = $"api/Test/Orders?$groupBy={WebUtility.UrlEncode("o => o.Price")};{WebUtility.UrlEncode("(k, g) => g.Count()")}";
                var content1 = await client.GetStringAsync(url1);
                var dynContent1 = JObject.Parse(content1);
                var counts = dynContent1["d"].ToObject<List<int>>();

                Assert.Equal(new[] { 1, 1, 2, 1 }, counts);

                var url2 = $"api/Test/Orders?$groupBy={WebUtility.UrlEncode("o => o.Price")}";
                var content2 = await client.GetStringAsync(url2);
                var dynContent2 = JObject.Parse(content2);
                var groups = dynContent2["d"].ToObject<List<List<Order>>>();

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
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Equal(4, Convert.ToInt32(dynContent["d"]));
            }
        }

        [Fact]
        public async Task ShouldSelectManyDetails() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$selectMany={WebUtility.UrlEncode("o => o.OrderDetails")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var details = dynContent["d"].ToObject<List<OrderDetail>>();

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
                var content1 = await client.GetStringAsync(url1);
                var dynContent1 = JObject.Parse(content1);

                Assert.Equal(3632f, float.Parse(dynContent1["d"].ToString()));

                var x = Consts.Orders.Select(o => o.Price).Aggregate(1f, (p1, p2) => p1 + p2);
                var url2 = $"api/Test/Orders?$select=Price&$aggregate={WebUtility.UrlEncode("(p1, p2) => p1 + p2")};10";
                var content2 = await client.GetStringAsync(url2);
                var dynContent2 = JObject.Parse(content2);

                Assert.Equal(3642f, float.Parse(dynContent2["d"].ToString()), 1);

                await Assert.ThrowsAsync<Exception>(() => client.GetStringAsync("api/Test/Orders?$select=Price&$aggregate=a;b;c"));
            }
        }


        [Fact]
        public async Task ShouldExecuteAll() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$all={WebUtility.UrlEncode("o => o.Id > 3")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Equal("false", dynContent["d"].ToString().ToLower());
            }
        }

        [Fact]
        public async Task ShouldExecuteAny() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$any={WebUtility.UrlEncode("o => o.Id < 3")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Equal("true", dynContent["d"].ToString().ToLower());
            }
        }

        [Fact]
        public async Task ShouldExecuteAvg() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$avg={WebUtility.UrlEncode("o => o.Id")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Equal(3, float.Parse(dynContent["d"].ToString()));
            }
        }

        [Fact]
        public async Task ShouldExecuteMax() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$max={WebUtility.UrlEncode("o => o.Price")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Equal(1125f, float.Parse(dynContent["d"].ToString()));
            }
        }

        [Fact]
        public async Task ShouldExecuteMin() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$min={WebUtility.UrlEncode("o => o.Price")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Equal(231.58f, float.Parse(dynContent["d"].ToString()));
            }
        }

        [Fact]
        public async Task ShouldExecuteSum() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$sum={WebUtility.UrlEncode("o => o.Price")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Equal(3632f, float.Parse(dynContent["d"].ToString()));
            }
        }

        [Fact]
        public async Task ShouldExecuteCount() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$count={WebUtility.UrlEncode("o => o.Id > 3")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Equal("2", dynContent["d"].ToString());
            }
        }

        [Fact]
        public async Task ShouldGetFirstOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$first={WebUtility.UrlEncode("o => o.Id > 3")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var order = dynContent["d"].ToObject<Order>();

                Assert.Equal(4, order.Id);
            }
        }

        [Fact]
        public async Task ShouldGetNullForNonExistingFirstOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$firstOrDefault={WebUtility.UrlEncode("o => o.Id > 5")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Empty(dynContent["d"].ToString());
            }
        }

        [Fact]
        public async Task ShouldGetSingleOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$single={WebUtility.UrlEncode("o => o.Id > 4")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var order = dynContent["d"].ToObject<Order>();

                Assert.Equal(5, order.Id);
            }
        }

        [Fact]
        public async Task ShouldGetNullForNonExistingSingleOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$singleOrDefault={WebUtility.UrlEncode("o => o.Id > 5")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Empty(dynContent["d"].ToString());
            }
        }

        [Fact]
        public async Task ShouldGetLastOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$last={WebUtility.UrlEncode("o => o.Id > 2")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var order = dynContent["d"].ToObject<Order>();

                Assert.Equal(5, order.Id);
            }
        }

        [Fact]
        public async Task ShouldGetNullForNonExistingLastOrder() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$lastOrDefault={WebUtility.UrlEncode("o => o.Id > 5")}";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Empty(dynContent["d"]);
            }
        }

        [Fact]
        public async Task ShouldGetElementAtIndex() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = "api/Test/Orders?$elementAt=3";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var order = dynContent["d"].ToObject<Order>();

                Assert.Equal(4, order.Id);
            }
        }

        [Fact]
        public async Task ShouldGetNullForNonExistingElementAtIndex() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$elementAtOrDefault=5";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Empty(dynContent["d"].ToString());
            }
        }

        [Fact]
        public async Task ShouldReverseOrders() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var url = $"api/Test/Orders?$reverse";
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);
                var orders = dynContent["d"].ToObject<List<Order>>();

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
                var content = await client.GetStringAsync(url);
                var dynContent = JObject.Parse(content);

                Assert.Equal("42", dynContent["d"].ToString());
            }
        }

        [Fact]
        public async Task ShouldThrowWhenMaxCountExceeded() {
            using (var testServer = CreateTestServer()) {
                var client = testServer.CreateClient();
                var content = await client.GetStringAsync("api/Test/LimitedOrders?$take=3");
                var dynContent = JObject.Parse(content);
                var orders = dynContent["d"].ToObject<List<Order>>();

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
