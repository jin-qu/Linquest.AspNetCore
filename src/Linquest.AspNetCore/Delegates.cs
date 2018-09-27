using System;
using System.Linq;

namespace Linquest.AspNetCore {

    public class BeforeQueryEventArgs : EventArgs {

        public BeforeQueryEventArgs(ActionContext context, IQueryable query) {
            Context = context;
            Query = query;
        }

        public ActionContext Context { get; }

        public IQueryable Query { get; set; }
    }

    public delegate void BeforeQueryDelegate(object sender, BeforeQueryEventArgs eventArgs);

    public class AfterQueryEventArgs : EventArgs {

        public AfterQueryEventArgs(ActionContext context, IQueryable query, object result) {
            Context = context;
            Query = query;
            Result = result;
        }

        public ActionContext Context { get; }

        public IQueryable Query { get; }

        public object Result { get; set; }
    }

    public delegate void AfterQueryDelegate(object sender, AfterQueryEventArgs eventArgs);
}