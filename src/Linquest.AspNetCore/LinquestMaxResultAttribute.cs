using System;

namespace Linquest.AspNetCore;

[AttributeUsage(AttributeTargets.Method)]
public class LinquestMaxResultAttribute(int count) : Attribute {
}
