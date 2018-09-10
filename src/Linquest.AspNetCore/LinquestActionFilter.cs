using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

// todo: MaxResult

namespace Linquest.AspNetCore {

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class LinquestActionFilterAttribute : ActionFilterAttribute {

        public LinquestActionFilterAttribute() : base() {
            Order = 0;
        }

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
            var actionContext = new ActionContext(
                context.ActionDescriptor, value, GetParameters(request)
            );

            var processResult = ProcessRequest(actionContext);
            context.Result = HandleResponse(processResult, response);
        }

        public virtual IReadOnlyList<LinquestParameter> GetParameters(HttpRequest request) {
            return Helper.GetParameters(request);
        }

        public virtual ProcessResult ProcessRequest(ActionContext actionContext) {
            return Helper.DefaultRequestProcessor(actionContext);
        }

        protected virtual ActionResult HandleResponse(ProcessResult result, HttpResponse response) => Helper.HandleResponse(result, response);
    }
}
