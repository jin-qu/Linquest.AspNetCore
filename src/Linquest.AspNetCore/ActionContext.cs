using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Linquest.AspNetCore {

    public class ActionContext {

        public ActionContext(ActionDescriptor descriptor, object value, IEnumerable<LinquestParameter> parameters) {
            Descriptor = descriptor;
            Value = value;
            Parameters = parameters;
        }

        public ActionDescriptor Descriptor { get; }
        public object Value { get; }
        public IEnumerable<LinquestParameter> Parameters  { get; }
    }
}
