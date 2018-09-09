using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using Linquest.AspNetCore;

namespace Linquest.AspNetCore {

    public class QueryableHandler {
        private static readonly Lazy<QueryableHandler> _instance = new Lazy<QueryableHandler>(() => new QueryableHandler());

        public virtual ProcessResult HandleContent(IQueryable query, ActionContext actionContext) {
            if (query == null) throw new ArgumentNullException(nameof(query));

            object result;
            int? inlineCount = null;

            var parameters = actionContext.Parameters;
            if (parameters != null) {
                var parameterList = parameters.ToList();
                (result, inlineCount) = HandleQuery(query, parameterList);
            } else {
                result = Enumerable.ToList((dynamic) query);
            }

            return new ProcessResult(actionContext) { Result = result, InlineCount = inlineCount };
        }

        public virtual(object, int?) HandleQuery(IQueryable query, IEnumerable < (string, string) > parameters) {
            var inlineCount = false;
            int? takeCount = null;
            IQueryable inlineCountQuery = null;
            foreach (var prm in parameters) {
                switch (prm.Item1.ToLowerInvariant()) {
                    case "inlinecount":
                        inlineCount = prm.Item2 == "allpages";
                        break;
                    case "oftype":
                        inlineCountQuery = null;
                        query = OfType(query, prm.Item2);
                        break;
                    case "filter":
                    case "where":
                        inlineCountQuery = null;
                        query = Where(query, prm.Item2);
                        break;
                    case "orderby":
                    case "thenby":
                        query = OrderBy(query, prm.Item2);
                        break;
                    case "expand":
                    case "include":
                        query = Include(query, prm.Item2);
                        break;
                    case "select":
                        query = Select(query, prm.Item2);
                        break;
                    case "skip":
                        query = Skip(query, Convert.ToInt32(prm.Item2));
                        break;
                    case "top":
                    case "take":
                        inlineCountQuery = query;
                        var take = Convert.ToInt32(prm.Item2);
                        query = Take(query, take);
                        takeCount = take;
                        break;
                    case "groupby":
                        inlineCountQuery = null;
                        var prms = prm.Item2.Split(';');
                        if (prms.Length > 2) throw new Exception("Invalid groupBy expression");

                        var keySelector = prms[0];
                        var valueSelector = prms.Length == 2 ? prms[1] : null;
                        query = GroupBy(query, keySelector, valueSelector);
                        break;
                    case "distinct":
                        inlineCountQuery = null;
                        query = Distinct(query, prm.Item2);
                        break;
                    case "reverse":
                        query = Reverse(query);
                        break;
                    case "selectmany":
                        inlineCountQuery = null;
                        takeCount = null;
                        query = SelectMany(query, prm.Item2);
                        break;
                    case "skipwhile":
                        query = SkipWhile(query, prm.Item2);
                        break;
                    case "takewhile":
                        inlineCountQuery = query;
                        query = TakeWhile(query, prm.Item2);
                        break;
                    case "all":
                        return CreateResult(All(query, prm.Item2), inlineCountQuery);
                    case "any":
                        return CreateResult(Any(query, prm.Item2), inlineCountQuery);
                    case "avg":
                        return CreateResult(Avg(query, prm.Item2), inlineCountQuery);
                    case "max":
                        return CreateResult(Max(query, prm.Item2), inlineCountQuery);
                    case "min":
                        return CreateResult(Min(query, prm.Item2), inlineCountQuery);
                    case "sum":
                        return CreateResult(Sum(query, prm.Item2), inlineCountQuery);
                    case "count":
                        return CreateResult(Count(query, prm.Item2), inlineCountQuery);
                    case "first":
                        return CreateResult(First(query, prm.Item2), inlineCountQuery);
                    case "firstordefault":
                        return CreateResult(FirstOrDefault(query, prm.Item2), inlineCountQuery);
                    case "single":
                        return CreateResult(Single(query, prm.Item2), inlineCountQuery);
                    case "singleordefault":
                        return CreateResult(SingleOrDefault(query, prm.Item2), inlineCountQuery);
                    case "last":
                        return CreateResult(Last(query, prm.Item2), inlineCountQuery);
                    case "lastordefault":
                        return CreateResult(LastOrDefault(query, prm.Item2), inlineCountQuery);
                    default:
                        throw new Exception($"Unknown query parameter {prm.Item1}");
                }
            }

            return CreateResult(query, inlineCountQuery);
        }

        private static(object, int?) CreateResult(object result, IQueryable inlineCountQuery) {
            return (
                result is IQueryable ? Enumerable.ToList((dynamic) result) : result,
                inlineCountQuery != null ? Queryable.Count((dynamic) inlineCountQuery) : null
            );
        }

        public virtual IQueryable OfType(IQueryable query, string ofType) {
            if (string.IsNullOrWhiteSpace(ofType)) return query;

            var elementType = query.ElementType;

            // use this type's namespace and assembly information to find wanted type
            var ofTypeFullName = $"{elementType.Namespace}.{ofType}, {elementType.GetTypeInfo().Assembly.GetName()}";
            var ofTypeType = Type.GetType(ofTypeFullName);
            if (ofTypeType == null)
                throw new ArgumentException($"Could not find type information for {ofTypeFullName}");

            // call Queryable's OfType method
            var mi = typeof(Queryable).GetMethod("OfType");
            var gmi = mi.MakeGenericMethod(ofTypeType);
            query = (IQueryable) gmi.Invoke(null, new object[] { query });
            return query;
        }

        public virtual IQueryable Where(IQueryable query, string filter) {
            return string.IsNullOrWhiteSpace(filter) ? query : query.Where(filter);
        }

        public virtual IQueryable OrderBy(IQueryable query, string orderBy) {
            return string.IsNullOrWhiteSpace(orderBy) ? query : query.OrderBy(orderBy);
        }

        public virtual IQueryable Include(IQueryable query, string expand) {
            if (string.IsNullOrWhiteSpace(expand)) return query;

            var dynQuery = (dynamic) query;
            expand.Split(',').ToList()
                .ForEach(e => { dynQuery = dynQuery.Include(e.Trim()); });
            return dynQuery;
        }

        public virtual IQueryable Select(IQueryable query, string projection) {
            if (string.IsNullOrWhiteSpace(projection)) return query;

            return projection.Contains(",") || projection.Contains(" as ") ?
                query.Select($"New({projection})") :
                query.Select(projection);
        }

        public virtual IQueryable Skip(IQueryable query, int count) {
            return query.Skip(count);
        }

        public virtual IQueryable Take(IQueryable query, int count) {
            return query.Take(count);
        }

        public virtual IQueryable GroupBy(IQueryable query, string keySelector, string elementSelector) {
            return query.GroupBy(keySelector, elementSelector);
        }

        public virtual IQueryable Distinct(IQueryable query, string elementSelector) {
            if (!string.IsNullOrWhiteSpace(elementSelector)) {
                query = Select(query, elementSelector);
            }

            return Queryable.Distinct((dynamic) query);
        }

        public virtual IQueryable Reverse(IQueryable query) {
            return Queryable.Reverse((dynamic) query);
        }

        public virtual IQueryable SelectMany(IQueryable query, string projection) {
            return string.IsNullOrWhiteSpace(projection) ? query : query.SelectMany(projection);
        }

        public virtual IQueryable SkipWhile(IQueryable query, string predicate) {
            return query.SkipWhile(predicate);
        }

        public virtual IQueryable TakeWhile(IQueryable query, string predicate) {
            return query.TakeWhile(predicate);
        }

        public virtual bool All(IQueryable query, string predicate) {
            return query.All(predicate);
        }

        public virtual bool Any(IQueryable query, string predicate) {
            if (!string.IsNullOrWhiteSpace(predicate)) {
                query = Where(query, predicate);
            }

            return query.Any();
        }

        public virtual object Avg(IQueryable query, string elementSelector) {
            if (!string.IsNullOrWhiteSpace(elementSelector)) {
                query = query.Select(elementSelector);
            }

            return Queryable.Average((dynamic) query);
        }

        public virtual object Max(IQueryable query, string elementSelector) {
            if (!string.IsNullOrWhiteSpace(elementSelector)) {
                query = query.Select(elementSelector);
            }

            return Queryable.Max((dynamic) query);
        }

        public virtual object Min(IQueryable query, string elementSelector) {
            if (!string.IsNullOrWhiteSpace(elementSelector)) {
                query = query.Select(elementSelector);
            }

            return Queryable.Min((dynamic) query);
        }

        public virtual object Sum(IQueryable query, string elementSelector) {
            if (!string.IsNullOrWhiteSpace(elementSelector)) {
                query = query.Select(elementSelector);
            }

            return Queryable.Sum((dynamic) query);
        }

        public virtual object Count(IQueryable query, string predicate) {
            if (!string.IsNullOrWhiteSpace(predicate)) {
                query = Where(query, predicate);
            }

            return Queryable.Count((dynamic) query);
        }

        public virtual object First(IQueryable query, string predicate) {
            if (!string.IsNullOrWhiteSpace(predicate)) {
                query = Where(query, predicate);
            }

            return Queryable.First((dynamic) query);
        }

        public virtual object FirstOrDefault(IQueryable query, string predicate) {
            if (!string.IsNullOrWhiteSpace(predicate)) {
                query = Where(query, predicate);
            }

            return Queryable.FirstOrDefault((dynamic) query);
        }

        public virtual object Single(IQueryable query, string predicate) {
            if (!string.IsNullOrWhiteSpace(predicate)) {
                query = Where(query, predicate);
            }

            return Queryable.Single((dynamic) query);
        }

        public virtual object SingleOrDefault(IQueryable query, string predicate) {
            if (!string.IsNullOrWhiteSpace(predicate)) {
                query = Where(query, predicate);
            }

            return Queryable.SingleOrDefault((dynamic) query);
        }

        public virtual object Last(IQueryable query, string predicate) {
            if (!string.IsNullOrWhiteSpace(predicate)) {
                query = Where(query, predicate);
            }

            return Queryable.Last((dynamic) query);
        }

        public virtual object LastOrDefault(IQueryable query, string predicate) {
            if (!string.IsNullOrWhiteSpace(predicate)) {
                query = Where(query, predicate);
            }

            return Queryable.LastOrDefault((dynamic) query);
        }

        public static QueryableHandler Instance => _instance.Value;
    }
}
