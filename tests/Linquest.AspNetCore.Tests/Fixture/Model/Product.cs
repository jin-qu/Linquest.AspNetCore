namespace Linquest.AspNetCore.Tests.Fixture.Model {

    public class Product {

        public Product(string no, string name, string category) {
            No = no;
            Name = name;
            Category = category;
        }

        public string No { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
    }
}
