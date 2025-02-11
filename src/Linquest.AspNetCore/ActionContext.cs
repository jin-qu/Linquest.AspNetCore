using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Linquest.AspNetCore;

using Interface;

public class ActionContext(ActionDescriptor? descriptor, object? value,
                           IEnumerable<LinquestParameter>? parameters,
                           ILinquestService? service) {

    public ActionDescriptor? Descriptor { get; } = descriptor;
    public object? Value { get; } = value;
    public IEnumerable<LinquestParameter> Parameters { get; } = parameters ?? [];
    public ILinquestService? Service { get; } = service;
    public int? MaxResultCount { get; set; }
}
