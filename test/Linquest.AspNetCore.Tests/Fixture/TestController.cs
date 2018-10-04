using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Linquest.AspNetCore.Tests.Fixture {
    using Model;

    [Route("api/[controller]")]
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

        [HttpGet]
        [Route("EnumerableOrders")]
        public IEnumerable<Order> EnumerableOrders() => Consts.Orders;

        [LinquestMaxResult(3)]
        [HttpGet]
        [Route("LimitedOrders")]
        public IQueryable<Order> LimitedOrders() => Consts.Orders.AsQueryable();

        [HttpGet]
        [Route("GetAProduct")]
        public Product GetAProduct() => new Product("P01", "Product01", "C01");

        [NonLinquestAction]
        [HttpGet]
        [Route("NonLinquestOrders")]
        public IQueryable<Order> NonLinquestOrders() => Consts.Orders.AsQueryable();

        [NonLinquestAction]
        [HttpGet]
        [Route("JsonOrders")]
        public JsonResult JsonOrders() => new JsonResult(Consts.Orders.ToList());

        protected override ProcessResult ProcessRequest(ActionContext actionContext) {
            if (actionContext.Descriptor == null)
                throw new ArgumentNullException(nameof(actionContext.Descriptor));

            return base.ProcessRequest(actionContext);
        }
    }
}
