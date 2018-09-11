namespace Linquest.AspNetCore.Tests.Fixture.Model {

    public class OrderDetail {

        public OrderDetail(string product, string supplier, int count) {
            Product = product;
            Supplier = supplier;
            Count = count;
        }

        public string Product { get; set; }
        public string Supplier { get; set; }
        public int Count { get; set; }
    }
}
