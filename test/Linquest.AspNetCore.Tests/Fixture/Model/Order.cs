using System;
using System.Collections.Generic;
using System.Linq;

namespace Linquest.AspNetCore.Tests.Fixture.Model;

public class Order(int id, string no, float price, DateTime date,
                   Customer customer, IEnumerable<OrderDetail> details) {
    public int Id { get; set; } = id;
    public string No { get; set; } = no;
    public float Price { get; set; } = price;
    public DateTime Date { get; set; } = date;
    public Customer Customer { get; set; } = customer;
    public IList<OrderDetail> OrderDetails { get; set; } = details?.ToList() ?? new List<OrderDetail>();
}
