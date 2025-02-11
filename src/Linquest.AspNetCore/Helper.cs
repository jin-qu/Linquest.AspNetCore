using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Linquest.AspNetCore;

using Interface;

public static class Helper {

    public static ProcessResult ProcessQuery(IQueryable query, HttpRequest request, ILinquestService? service = null) {
        var parameters = GetParameters(request);
        var actionContext = new ActionContext(null, query, parameters, service);

        return DefaultRequestProcessor(actionContext);
    }

    public static IReadOnlyList<LinquestParameter> GetParameters(HttpRequest request) {
        return request.Query
            .Where(q => q.Key.StartsWith("$"))
            .Select(q => new LinquestParameter(q.Key, q.Value.ToString()))
            .ToList()
            .AsReadOnly();
    }

    public static ProcessResult DefaultRequestProcessor(ActionContext context, IServiceProvider? serviceProvider = null) {
        var value = context.Value;
        if (value == null)
            return new ProcessResult(context) { Result = null };

        var handlerType = typeof(IContentHandler<>).MakeGenericType(value.GetType());
        var handler = serviceProvider?.GetService(handlerType);

        if (handler != null)
            return ((IContentHandler)handler).HandleContent(value, context);

        if (context.Value is IQueryable queryable)
            return QueryableHandler.Instance.HandleContent(queryable, context);

        return new ProcessResult(context) { Result = value };
    }
}
