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

        public LinquestActionFilterAttribute(int maxResultCount) {
            Order = 0;
            MaxResultCount = maxResultCount;
        }

        public int? MaxResultCount { get; }

        public override void OnResultExecuting(ResultExecutingContext context) {
            base.OnResultExecuting(context);

            if (!(context.Result is ObjectResult objectResult)) return;

            if (context.ActionDescriptor is ControllerActionDescriptor cad &&
                cad.MethodInfo.CustomAttributes.Any(a => a.AttributeType == typeof(NonLinquestActionAttribute))) return;

            var value = objectResult.Value;
            var service = context.Controller as ILinquestService;
            var request = context.HttpContext.Request;
            var response = context.HttpContext.Response;

            // translate the request query
            var actionContext = new ActionContext(context.ActionDescriptor, value, GetParameters(request), service) {
                MaxResultCount = MaxResultCount
            };
            var processResult = ProcessRequest(actionContext, context.HttpContext.RequestServices);
            context.Result = HandleResponse(processResult, response);
        }

        protected virtual IReadOnlyList<LinquestParameter> GetParameters(HttpRequest request) 
            => Helper.GetParameters(request);

        protected virtual ProcessResult ProcessRequest(ActionContext actionContext, IServiceProvider serviceProvider) 
            => Helper.DefaultRequestProcessor(actionContext, serviceProvider);

        protected virtual ActionResult HandleResponse(ProcessResult result, HttpResponse response) 
            => Helper.HandleResponse(result, response);
    }
}
