using System;
using System.Collections.Generic;
using System.Linq;

namespace Linquest.AspNetCore.Tests.Fixture.Model {

    public class Order {

        public Order(int id, string no, float price, DateTime date,
            Customer customer, IEnumerable<OrderDetail> details) {
            Id = id;
            No = no;
            Price = price;
            Date = date;
            Customer = customer;
            OrderDetails = details?.ToList() ?? new List<OrderDetail>();
        }

        public int Id { get; set; }
        public string No { get; set; }
        public float Price { get; set; }
        public DateTime Date { get; set; }
        public Customer Customer { get; set; }
        public IList<OrderDetail> OrderDetails { get; set; }
    }
}
