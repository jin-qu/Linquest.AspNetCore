namespace Linquest.AspNetCore.Tests.Fixture.Model;

public class OrderDetail(string product, string supplier, int count) {
    public string Product { get; set; } = product;
    public string Supplier { get; set; } = supplier;
    public int Count { get; set; } = count;
}
