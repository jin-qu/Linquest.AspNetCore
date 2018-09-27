using System;
using System.Linq;
using Linquest.AspNetCore.Tests.Fixture.Model;
using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore.Tests.Fixture {

    [Route("api/[controller]")]
    [LinquestActionFilter]
    public class TestController : LinquestController {

        public TestController() {
            BeforeHandleQuery += (sender, args) => {
                if (args.Context == null)
                    throw new ArgumentNullException(nameof(args.Context));
            };

            AfterQueryExecute += (sender, args) => {
                if (args.Context == null)
                    throw new ArgumentNullException(nameof(args.Context));

                if (args.Query == null)
                    throw new ArgumentNullException(nameof(args.Context));
            };
        }

        [HttpGet]
        public string Get() {
            return "Hello World!";
        }

        [LinquestMaxResult(3)]
        [HttpGet]
        [Route("NullValue")]
        public IQueryable<Order> NullValue() => null;

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
