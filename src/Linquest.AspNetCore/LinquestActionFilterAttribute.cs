using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

// todo: IAsyncResultFilter?

namespace Linquest.AspNetCore {
    using Interface;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class LinquestActionFilterAttribute : ActionFilterAttribute {

        public LinquestActionFilterAttribute() {
            Order = 0;
        }

        public override void OnResultExecuting(ResultExecutingContext context) {
            base.OnResultExecuting(context);

            if (!(context.Result is ObjectResult objectResult)) 
                return;

            var cad = context.ActionDescriptor as ControllerActionDescriptor;
            if (cad != null && cad.MethodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(NonLinquestActionAttribute))) 
                return;

            var value = objectResult.Value;
            var service = context.Controller as ILinquestService;
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;

            // translate the request query
            var maxAttr = cad?.MethodInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(LinquestMaxResultAttribute));
            var max = (int?)maxAttr?.ConstructorArguments.First().Value;
            var ac = new ActionContext(context.ActionDescriptor, value, GetParameters(request), service)
            {
                MaxResultCount = max
            };
            var processResult = ProcessRequest(ac, context.HttpContext.RequestServices);
            context.Result = HandleResponse(processResult, response);
        }

        protected virtual IReadOnlyList<LinquestParameter> GetParameters(HttpRequest request)
            => Helper.GetParameters(request);

        protected virtual ProcessResult ProcessRequest(ActionContext context, IServiceProvider serviceProvider)
            => Helper.DefaultRequestProcessor(context, serviceProvider);

        protected virtual ActionResult HandleResponse(ProcessResult result, HttpResponse response)
            => Helper.HandleResponse(result, response);
    }
}
