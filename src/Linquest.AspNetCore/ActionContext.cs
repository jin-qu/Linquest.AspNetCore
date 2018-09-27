using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Linquest.AspNetCore {
    using Interface;

    public class ActionContext {

        public ActionContext(ActionDescriptor descriptor, object value, IEnumerable<LinquestParameter> parameters, ILinquestService service) {
            Descriptor = descriptor;
            Value = value;
            Parameters = parameters;
            Service = service;
        }

        public ActionDescriptor Descriptor { get; }
        public object Value { get; }
        public IEnumerable<LinquestParameter> Parameters { get; }
        public ILinquestService Service { get; }
        public int? MaxResultCount { get; set; }
    }
}
