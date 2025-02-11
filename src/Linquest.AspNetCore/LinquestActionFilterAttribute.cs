using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Linquest.AspNetCore;

using Interface;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class LinquestActionFilterAttribute : ActionFilterAttribute {
    public LinquestActionFilterAttribute() => Order = 0;

    public override void OnResultExecuting(ResultExecutingContext context) {
        base.OnResultExecuting(context);

        if (context.Result is not ObjectResult objectResult)
            return;

        var cad = context.ActionDescriptor as ControllerActionDescriptor;
        var nqt = typeof(NonLinquestActionAttribute);
        if (cad != null && cad.MethodInfo.CustomAttributes.Any(a => a.AttributeType == nqt))
            return;

        var value = objectResult.Value;
        var service = context.Controller as ILinquestService;
        var request = context.HttpContext.Request;
        var response = context.HttpContext.Response;

        // translate the request query
        var mra = typeof(LinquestMaxResultAttribute);
        var maxAttr = cad?.MethodInfo.CustomAttributes
            .FirstOrDefault(a => a.AttributeType == mra);
        var max = (int?)maxAttr?.ConstructorArguments.First()
            .Value;
        var ac = new ActionContext(context.ActionDescriptor, value, GetParameters(request), service) {
            MaxResultCount = max
        };
        var processResult = ProcessRequest(ac, context.HttpContext.RequestServices);
        context.Result = HandleResponse(processResult, response);
    }

    protected virtual IReadOnlyList<LinquestParameter> GetParameters(HttpRequest request)
        => Helper.GetParameters(request);

    protected virtual ProcessResult ProcessRequest(ActionContext context, IServiceProvider serviceProvider)
        => context.Service != null
            ? context.Service.ProcessRequest(context)
            : Helper.DefaultRequestProcessor(context, serviceProvider);

    protected virtual ActionResult HandleResponse(ProcessResult result, HttpResponse response)
        => result.InlineCount != null
            ? new ObjectResult(new { d = result.Result, inlineCount = result.InlineCount })
            : new ObjectResult(new { d = result.Result });
}
