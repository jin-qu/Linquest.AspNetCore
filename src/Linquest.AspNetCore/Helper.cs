using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore {

    public static class Helper {

        public static IList < (string, string) > GetParameters(HttpRequest request) {
            return request.Query
                .Where(q => q.Key.StartsWith("$"))
                .Select(q =>(q.Key, q.Value.ToString()))
                .ToList();
        }

        public static ProcessResult DefaultRequestProcessor(ActionContext context) {
            var value = context.Value;
            if (context.Value is IQueryable queryable) {
                return QueryableHandler.Instance.HandleContent(queryable, context);
            }

            if (value is string || !(value is IEnumerable))
                return new ProcessResult(context) { Result = value };

            return new ProcessResult(context) { Result = value };
        }

        // todo: JSON Array Ctor Vulnerability, wrap arrays with and object
        public static ActionResult HandleResponse(ProcessResult result, HttpResponse response) {
            var inlineCount = result.InlineCount;
            if (inlineCount != null && !response.Headers.ContainsKey("X-InlineCount")) {
            response.Headers.Add("X-InlineCount", inlineCount.ToString());
            }

            return new ObjectResult(result.Result);
        }
    }
}
