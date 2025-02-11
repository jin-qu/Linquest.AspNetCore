namespace Linquest.AspNetCore.Tests.Fixture.Model;

public class Product(string no, string name, string category) {
    public string No { get; set; } = no;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
}
