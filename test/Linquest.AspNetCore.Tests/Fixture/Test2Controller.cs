using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore.Tests.Fixture {
    using Model;

    [Route("api/[controller]")]
    [LinquestActionFilter]
    public class Test2Controller {

        [HttpGet]
        [Route("Orders")]
        public IQueryable<Order> Orders() => Consts.Orders.AsQueryable();
    }
}
