using System.Linq;
using Linquest.AspNetCore.Tests.Fixture.Model;
using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore.Tests.Fixture {

    [Route("api/[controller]")]
    public class TestController : LinquestController {

        [HttpGet]
        public string Get() {
            return "Hello World!";
        }

        [LinquestActionFilter]
        [HttpGet]
        [Route("Orders")]
        public IQueryable<Order> Orders() => Consts.Orders.AsQueryable();

        [LinquestActionFilter(3)]
        [HttpGet]
        [Route("LimitedOrders")]
        public IQueryable<Order> LimitedOrders() => Consts.Orders.AsQueryable();
    }
}
