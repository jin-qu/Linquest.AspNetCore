using System;
using System.Linq;

namespace Linquest.AspNetCore {

    public class BeforeQueryEventArgs : EventArgs {

        public BeforeQueryEventArgs(ActionContext actionContext, IQueryable query) {
            ActionContext = actionContext;
            Query = query;
        }

        public ActionContext ActionContext { get; }

        public IQueryable Query { get; set; }
    }

    public delegate void BeforeQueryDelegate(object sender, BeforeQueryEventArgs eventArgs);

    public class AfterQueryEventArgs : EventArgs {

        public AfterQueryEventArgs(ActionContext actionContext, IQueryable query, object result) {
            ActionContext = actionContext;
            Query = query;
            Result = result;
        }

        public ActionContext ActionContext { get; }

        public IQueryable Query { get; }

        public object Result { get; set; }
    }

    public delegate void AfterQueryDelegate(object sender, AfterQueryEventArgs eventArgs);
}