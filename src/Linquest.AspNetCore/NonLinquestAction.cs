using System;

namespace Linquest.AspNetCore;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.All)]
public class NonLinquestActionAttribute: Attribute {
}