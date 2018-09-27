using System.Linq;

namespace Linquest.AspNetCore {

    public class HandledQuery {

        public HandledQuery(IQueryable query, IQueryable inlineCountQuery, int? takeCount = null) {
            Query = query;
            InlineCountQuery = inlineCountQuery;
            TakeCount = takeCount;
        }

        public IQueryable Query { get; }

        public IQueryable InlineCountQuery { get; }

        public int? TakeCount { get; }
    }
}
