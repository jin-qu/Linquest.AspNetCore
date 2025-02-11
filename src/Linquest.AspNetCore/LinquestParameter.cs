namespace Linquest.AspNetCore;

public readonly struct LinquestParameter(string name, string value) {
    public string Name { get; } = name;
    public string Value { get; } = value;
}
