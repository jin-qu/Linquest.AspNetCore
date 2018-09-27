using System;
using System.Linq;
using Linquest.AspNetCore.Tests.Fixture.Model;
using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore.Tests.Fixture {

    [Route("api/[controller]")]
    [LinquestActionFilter]
    public class TestController : LinquestController {

        [HttpGet]
        public string Get() {
            return "Hello World!";
        }

        [HttpGet]
        [Route("Orders")]
        public IQueryable<Order> Orders() => Consts.Orders.AsQueryable();

        [LinquestMaxResult(3)]
        [HttpGet]
        [Route("LimitedOrders")]
        public IQueryable<Order> LimitedOrders() => Consts.Orders.AsQueryable();

        [NonLinquestAction]
        [HttpGet]
        [Route("NonLinquestOrders")]
        public IQueryable<Order> NonLinquestOrders() => Consts.Orders.AsQueryable();

        protected override ProcessResult ProcessRequest(ActionContext actionContext) {
            if (actionContext.Descriptor == null)
                throw new ArgumentNullException(nameof(actionContext.Descriptor));

            return base.ProcessRequest(actionContext);
        }
    }
}
