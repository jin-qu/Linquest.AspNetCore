using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore.Tests.Fixture;

using Model;

[Route("api/[controller]")]
public class TestController : LinquestController {

    public TestController() {
        MaxResultCount = 100;

        BeforeHandleQuery += (_, args) => {
            args.Query = args.Query;

            if (args.Context == null)
                throw new ArgumentNullException(nameof(args.Context));
        };

        AfterQueryExecute += (_, args) => {
            args.Result = args.Result;

            if (args.Context == null)
                throw new ArgumentNullException(nameof(args.Context));

            if (args.Query == null)
                throw new ArgumentNullException(nameof(args.Context));
        };
    }

    [HttpGet]
    [NonLinquestAction]
    public string Get() => "Hello World!";

    [LinquestMaxResult(3)]
    [HttpGet]
    [Route("NullValue")]
    public IQueryable<Order>? NullValue() => null;

    [HttpGet]
    [Route("Orders")]
    public IQueryable<Order> Orders() => Constants.Orders.AsQueryable();

    [HttpGet]
    [Route("EnumerableOrders")]
    public IEnumerable<Order> EnumerableOrders() => Constants.Orders;

    [LinquestMaxResult(3)]
    [HttpGet]
    [Route("LimitedOrders")]
    public IQueryable<Order> LimitedOrders() => Constants.Orders.AsQueryable();

    [HttpGet]
    [Route("GetAProduct")]
    public Product GetAProduct() => new Product("P01", "Product01", "C01");

    [NonLinquestAction]
    [HttpGet]
    [Route("NonLinquestOrders")]
    public IQueryable<Order> NonLinquestOrders() => Constants.Orders.AsQueryable();

    [NonLinquestAction]
    [HttpGet]
    [Route("JsonOrders")]
    public JsonResult JsonOrders() => new JsonResult(Constants.Orders.ToList());

    protected override ProcessResult ProcessRequest(ActionContext actionContext) {
        if (actionContext.Descriptor == null)
            throw new ArgumentNullException(nameof(actionContext.Descriptor));

        return base.ProcessRequest(actionContext);
    }
}
