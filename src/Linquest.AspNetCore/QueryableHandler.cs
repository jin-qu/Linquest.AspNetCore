using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using Linquest.AspNetCore;

namespace Linquest.AspNetCore
{
    using Interface;

    public class QueryableHandler : IContentHandler<IQueryable>
    {
        private static readonly Lazy<QueryableHandler> _instance = new Lazy<QueryableHandler>();

        public virtual ProcessResult HandleContent(object query, ActionContext context)
        {
            return HandleContent((IQueryable) query, context);
        }

        public virtual ProcessResult HandleContent(IQueryable query, ActionContext context)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var service = context.Service;
            if (service != null)
            {
                var args = new BeforeQueryEventArgs(context, query);
                service.OnBeforeHandleQuery(args);
                query = args.Query;
            }

            var parameters = context.Parameters;
            if (parameters == null || !parameters.Any())
                return CreateResult(context, query, null, null);

            var inlineCountEnabled = false;
            int? takeCount = null;
            IQueryable inlineCountQuery = null;
            foreach (var prm in parameters)
            {
                switch (prm.Name.ToLowerInvariant())
                {
                    case "$inlinecount":
                        inlineCountEnabled = prm.Value != "false" || prm.Value == "allpages";
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
                        if (inlineCountEnabled && inlineCountQuery == null)
                        {
                            inlineCountQuery = query;
                        }

                        query = Skip(query, Convert.ToInt32(prm.Value));
                        break;
                    case "$top":
                    case "$take":
                        if (inlineCountEnabled && inlineCountQuery == null)
                        {
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

        public virtual IQueryable Where(IQueryable query, string filter)
        {
            return query.Where(filter);
        }

        public virtual IQueryable OrderBy(IQueryable query, string orderBy)
        {
            return query.OrderBy(orderBy);
        }

        public virtual IQueryable OrderbyDescending(IQueryable query, string orderBy)
        {
            return query.OrderByDescending(orderBy);
        }

        public virtual IQueryable ThenBy(IQueryable query, string orderBy)
        {
            return query.ThenBy(orderBy);
        }

        public virtual IQueryable ThenByDescending(IQueryable query, string orderBy)
        {
            return query.ThenByDescending(orderBy);
        }

        public virtual IQueryable Select(IQueryable query, string projection)
        {
            return query.Select(projection);
        }

        public virtual IQueryable Skip(IQueryable query, int count)
        {
            return query.Skip(count);
        }

        public virtual IQueryable Take(IQueryable query, int count)
        {
            return query.Take(count);
        }

        public virtual IQueryable GroupBy(IQueryable query, string keySelector, string elementSelector)
        {
            return string.IsNullOrWhiteSpace(elementSelector)
#if NETSTANDARD2_0
                ? query.GroupBy(keySelector)
#elif NETCOREAPP3_0
                //INFO: List<IGrouping<Item>> can't be serialized using default ASP.NET Core 3.0 serializer
                ? query.GroupBy(keySelector, "(key, group) => group")
#endif
                : query.GroupBy(keySelector, elementSelector);
        }

        public virtual IQueryable Distinct(IQueryable query)
        {
            return query.Distinct();
        }

        public virtual IQueryable Reverse(IQueryable query)
        {
            return Queryable.Reverse((dynamic) query);
        }

        public virtual IQueryable SelectMany(IQueryable query, string projection)
        {
            return string.IsNullOrWhiteSpace(projection) ? query : query.SelectMany(projection);
        }

        public virtual IQueryable SkipWhile(IQueryable query, string predicate)
        {
            return query.SkipWhile(predicate);
        }

        public virtual IQueryable TakeWhile(IQueryable query, string predicate)
        {
            return query.TakeWhile(predicate);
        }

        public virtual object Aggregate(IQueryable query, string func, string seed = null)
        {
            return string.IsNullOrWhiteSpace(seed)
                ? query.Aggregate(func)
                : query.Aggregate(Convert.ToDouble(seed), func);
        }

        public virtual bool All(IQueryable query, string predicate)
        {
            return query.All(predicate);
        }

        public virtual bool Any(IQueryable query, string predicate)
        {
            return query.Any(predicate);
        }

        public virtual object Avg(IQueryable query, string elementSelector)
        {
            return query.Average(elementSelector);
        }

        public virtual object Max(IQueryable query, string elementSelector)
        {
            return query.Max(elementSelector);
        }

        public virtual object Min(IQueryable query, string elementSelector)
        {
            return query.Min(elementSelector);
        }

        public virtual object Sum(IQueryable query, string elementSelector)
        {
            return query.Sum(elementSelector);
        }

        public virtual object Count(IQueryable query, string predicate)
        {
            return query.Count(predicate);
        }

        public virtual object First(IQueryable query, string predicate)
        {
            return query.First(predicate);
        }

        public virtual object FirstOrDefault(IQueryable query, string predicate)
        {
            return query.FirstOrDefault(predicate);
        }

        public virtual object Single(IQueryable query, string predicate)
        {
            return query.Single(predicate);
        }

        public virtual object SingleOrDefault(IQueryable query, string predicate)
        {
            return query.SingleOrDefault(predicate);
        }

        public virtual object Last(IQueryable query, string predicate)
        {
            return query.Last(predicate);
        }

        public virtual object LastOrDefault(IQueryable query, string predicate)
        {
            return query.LastOrDefault(predicate);
        }

        public virtual object ElementAt(IQueryable query, int index)
        {
            return query.ElementAt(index);
        }

        public virtual object ElementAtOrDefault(IQueryable query, int index)
        {
            return query.ElementAtOrDefault(index);
        }

        protected static ProcessResult CreateResult(ActionContext context, IQueryable query, int? takeCount, IQueryable inlineCountQuery)
        {
            int? max = context.MaxResultCount ?? context.Service?.MaxResultCount;
            if (max > 0)
            {
                var count = takeCount ?? Queryable.Count((dynamic) query);
                if (count > max) throw new Exception($"Maximum allowed read count exceeded");
            }

            var service = context.Service;
            if (service == null)
                return CreateResult(context, Enumerable.ToList((dynamic) query), inlineCountQuery);

            var beforeArgs = new BeforeQueryEventArgs(context, query);
            service.OnBeforeQueryExecute(beforeArgs);
            query = beforeArgs.Query;

            var result = Enumerable.ToList((dynamic) query);
            var afterArgs = new AfterQueryEventArgs(context, query, result);
            service.OnAfterQueryExecute(afterArgs);
            result = afterArgs.Result;

            return CreateResult(context, result, inlineCountQuery);
        }

        protected static ProcessResult CreateResult(ActionContext context, object result, IQueryable inlineCountQuery)
        {
            int? inlineCount = inlineCountQuery != null ? Queryable.Count((dynamic) inlineCountQuery) : null;
            return new ProcessResult(context) {Result = result, InlineCount = inlineCount};
        }

        public static QueryableHandler Instance => _instance.Value;
    }
}