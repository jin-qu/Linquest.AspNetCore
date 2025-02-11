using System;
using System.Linq;

namespace Linquest.AspNetCore;

public class BeforeQueryEventArgs(ActionContext context, IQueryable query) : EventArgs {
    public ActionContext Context { get; } = context;

    public IQueryable Query { get; set; } = query;
}

public delegate void BeforeQueryDelegate(object sender, BeforeQueryEventArgs eventArgs);

public class AfterQueryEventArgs(ActionContext context, IQueryable query, object result) : EventArgs {
    public ActionContext Context { get; } = context;

    public IQueryable Query { get; } = query;

    public object Result { get; set; } = result;
}

public delegate void AfterQueryDelegate(object sender, AfterQueryEventArgs eventArgs);
