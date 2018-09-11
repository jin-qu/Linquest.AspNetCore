using System.Linq;
using Linquest.AspNetCore.Tests.Fixture.Model;
using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore.Tests.Fixture {

    [Route("api/[controller]")]
    public class TestController : Controller {

        [HttpGet]
        public string Get() {
            return "Hello World!";
        }

        [LinquestActionFilter]
        [HttpGet]
        [Route("Orders")]
        public IQueryable<Order> Orders() => Consts.Orders.AsQueryable();
    }
}
