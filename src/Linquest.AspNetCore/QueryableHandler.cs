using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace Linquest.AspNetCore;

using Interface;

public class QueryableHandler : IContentHandler<IQueryable> {
    private static readonly Lazy<QueryableHandler> _instance = new();
    public static QueryableHandler Instance => _instance.Value;

    public virtual ProcessResult HandleContent(object query, ActionContext context) =>
        HandleContent((IQueryable)query, context);

    public virtual ProcessResult HandleContent(IQueryable query, ActionContext context) {
        if (query is null) throw new ArgumentNullException(nameof(query));
        if (context is null) throw new ArgumentNullException(nameof(context));

        var service = context.Service;
        if (service != null) {
            var args = new BeforeQueryEventArgs(context, query);
            service.OnBeforeHandleQuery(args);
            query = args.Query;
        }

        var prmEnum = context.Parameters;
        var parameters = prmEnum as IList<LinquestParameter> ?? prmEnum.ToList();
        if (!parameters.Any())
            return CreateResult(context, query, null, null);

        var inlineCountEnabled = false;
        int? takeCount = null;
        IQueryable? inlineCountQuery = null;
        foreach (var prm in parameters) {
            switch (prm.Name.ToLowerInvariant()) {
                case "$inlinecount":
                    inlineCountEnabled = prm.Value is null or "" or "true" or "allpages";
                    break;
                case "$filter":
                case "$where":
                    inlineCountQuery = null;
                    query = Where(query, prm.Value);
                    break;
                case "$orderby":
                    query = OrderBy(query, prm.Value);
                    break;
                case "$orderbydescending":
                    query = OrderbyDescending(query, prm.Value);
                    break;
                case "$thenby":
                    query = ThenBy(query, prm.Value);
                    break;
                case "$thenbydescending":
                    query = ThenByDescending(query, prm.Value);
                    break;
                case "$select":
                    query = Select(query, prm.Value);
                    break;
                case "$skip":
                    if (inlineCountEnabled && inlineCountQuery == null) {
                        inlineCountQuery = query;
                    }

                    query = Skip(query, Convert.ToInt32(prm.Value));
                    break;
                case "$top":
                case "$take":
                    if (inlineCountEnabled && inlineCountQuery == null) {
                        inlineCountQuery = query;
                    }

                    var take = Convert.ToInt32(prm.Value);
                    query = Take(query, take);
                    takeCount = take;
                    break;
                case "$groupby":
                    inlineCountQuery = null;
                    var keyValue = prm.Value.Split(';');
                    if (keyValue.Length > 2) throw new Exception("Invalid groupBy expression");

                    var keySelector = keyValue[0];
                    var valueSelector = keyValue.Length == 2 ? keyValue[1] : null;
                    query = GroupBy(query, keySelector, valueSelector);
                    break;
                case "$distinct":
                    inlineCountQuery = null;
                    query = Distinct(query);
                    break;
                case "$reverse":
                    query = Reverse(query);
                    break;
                case "$selectmany":
                    inlineCountQuery = null;
                    takeCount = null;
                    query = SelectMany(query, prm.Value);
                    break;
                case "$skipwhile":
                    inlineCountQuery = null;
                    query = SkipWhile(query, prm.Value);
                    break;
                case "$takewhile":
                    inlineCountQuery = null;
                    query = TakeWhile(query, prm.Value);
                    break;
                case "$aggregate":
                    var funcSeed = prm.Value.Split(';');
                    if (funcSeed.Length > 2) throw new Exception("Invalid aggregate expression");

                    var func = funcSeed[0];
                    var seed = funcSeed.Length == 2 ? funcSeed[1] : null;
                    return CreateResult(context, Aggregate(query, func, seed), inlineCountQuery);
                case "$all":
                    return CreateResult(context, All(query, prm.Value), inlineCountQuery);
                case "$any":
                    return CreateResult(context, Any(query, prm.Value), inlineCountQuery);
                case "$average":
                case "$avg":
                    return CreateResult(context, Avg(query, prm.Value), inlineCountQuery);
                case "$max":
                    return CreateResult(context, Max(query, prm.Value), inlineCountQuery);
                case "$min":
                    return CreateResult(context, Min(query, prm.Value), inlineCountQuery);
                case "$sum":
                    return CreateResult(context, Sum(query, prm.Value), inlineCountQuery);
                case "$count":
                    return CreateResult(context, Count(query, prm.Value), inlineCountQuery);
                case "$first":
                    return CreateResult(context, First(query, prm.Value), inlineCountQuery);
                case "$firstordefault":
                    return CreateResult(context, FirstOrDefault(query, prm.Value), inlineCountQuery);
                case "$single":
                    return CreateResult(context, Single(query, prm.Value), inlineCountQuery);
                case "$singleordefault":
                    return CreateResult(context, SingleOrDefault(query, prm.Value), inlineCountQuery);
                case "$last":
                    return CreateResult(context, Last(query, prm.Value), inlineCountQuery);
                case "$lastordefault":
                    return CreateResult(context, LastOrDefault(query, prm.Value), inlineCountQuery);
                case "$elementat":
                    return CreateResult(context, ElementAt(query, Convert.ToInt32(prm.Value)), inlineCountQuery);
                case "$elementatordefault":
                    return CreateResult(context, ElementAtOrDefault(query, Convert.ToInt32(prm.Value)), inlineCountQuery);
                default:
                    throw new Exception($"Unknown query parameter {prm.Value}");
            }
        }

        return CreateResult(context, query, takeCount, inlineCountQuery);
    }

    protected virtual IQueryable Where(IQueryable query, string filter) =>
        query.Where(filter);

    protected virtual IQueryable OrderBy(IQueryable query, string orderBy) =>
        query.OrderBy(orderBy);

    protected virtual IQueryable OrderbyDescending(IQueryable query, string orderBy) =>
        query.OrderByDescending(orderBy);

    protected virtual IQueryable ThenBy(IQueryable query, string orderBy) =>
        query.ThenBy(orderBy);

    protected virtual IQueryable ThenByDescending(IQueryable query, string orderBy) =>
        query.ThenByDescending(orderBy);

    protected virtual IQueryable Select(IQueryable query, string projection) =>
        query.Select(projection);

    protected virtual IQueryable Skip(IQueryable query, int count) =>
        query.Skip(count);

    protected virtual IQueryable Take(IQueryable query, int count) =>
        query.Take(count);

    protected virtual IQueryable GroupBy(IQueryable query, string keySelector, string? elementSelector) {
        return string.IsNullOrWhiteSpace(elementSelector)
            ? query.GroupBy(keySelector, "(key, group) => group")
            : query.GroupBy(keySelector, elementSelector);
    }

    protected virtual IQueryable Distinct(IQueryable query) =>
        query.Distinct();

    protected virtual IQueryable Reverse(IQueryable query) =>
        query.Reverse();

    protected virtual IQueryable SelectMany(IQueryable query, string projection) =>
        string.IsNullOrWhiteSpace(projection) ? query : query.SelectMany(projection);

    protected virtual IQueryable SkipWhile(IQueryable query, string predicate) =>
        query.SkipWhile(predicate);

    protected virtual IQueryable TakeWhile(IQueryable query, string predicate) =>
        query.TakeWhile(predicate);

    protected virtual object Aggregate(IQueryable query, string func, string? seed = null) =>
        string.IsNullOrWhiteSpace(seed)
            ? query.Aggregate(func)
            : query.Aggregate(Convert.ToDouble(seed), func);

    protected virtual bool All(IQueryable query, string predicate) =>
        query.All(predicate);

    protected virtual bool Any(IQueryable query, string predicate) =>
        query.Any(predicate);

    protected virtual object Avg(IQueryable query, string elementSelector) =>
        query.Average(elementSelector);

    protected virtual object Max(IQueryable query, string elementSelector) =>
        query.Max(elementSelector);

    protected virtual object Min(IQueryable query, string elementSelector) =>
        query.Min(elementSelector);

    protected virtual object Sum(IQueryable query, string elementSelector) =>
        query.Sum(elementSelector);

    protected virtual object Count(IQueryable query, string predicate) =>
        query.Count(predicate);

    protected virtual object First(IQueryable query, string predicate) =>
        query.First(predicate);

    protected virtual object? FirstOrDefault(IQueryable query, string? predicate) =>
        query.FirstOrDefault(predicate);

    protected virtual object Single(IQueryable query, string? predicate) =>
        query.Single(predicate);

    protected virtual object? SingleOrDefault(IQueryable query, string? predicate) =>
        query.SingleOrDefault(predicate);

    protected virtual object Last(IQueryable query, string? predicate) =>
        query.Last(predicate);

    protected virtual object? LastOrDefault(IQueryable query, string? predicate) =>
        query.LastOrDefault(predicate);

    protected virtual object ElementAt(IQueryable query, int index) =>
        query.ElementAt(index);

    protected virtual object? ElementAtOrDefault(IQueryable query, int index) =>
        query.ElementAtOrDefault(index);

    protected static ProcessResult CreateResult(ActionContext context, IQueryable query, int? takeCount, IQueryable? inlineCountQuery) {
        var max = context.MaxResultCount ?? context.Service?.MaxResultCount;
        if (max > 0) {
            var count = takeCount ?? query.Count();
            if (count > max) throw new Exception($"Maximum allowed read count exceeded");
        }

        var service = context.Service;
        if (service == null)
            return CreateResult(context, Enumerable.ToList((dynamic)query), inlineCountQuery);

        var beforeArgs = new BeforeQueryEventArgs(context, query);
        service.OnBeforeQueryExecute(beforeArgs);
        query = beforeArgs.Query;

        object result = Enumerable.ToList((dynamic)query);
        var afterArgs = new AfterQueryEventArgs(context, query, result);
        service.OnAfterQueryExecute(afterArgs);
        result = afterArgs.Result;

        return CreateResult(context, result, inlineCountQuery);
    }

    protected static ProcessResult CreateResult(ActionContext context, object? result, IQueryable? inlineCountQuery) {
        var inlineCount = inlineCountQuery?.Count();
        return new ProcessResult(context) { Result = result, InlineCount = inlineCount };
    }
}
