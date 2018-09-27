using System.Collections.Generic;

namespace Linquest.AspNetCore.Tests.Fixture {
    using System;
    using Model;

    internal static class Consts {

        internal static List<Order> Orders = new List<Order> {
            new Order(1, "Ord1", 400, new DateTime(2013, 8, 6, 12, 34, 56), new Customer("Cus4"),
                new List<OrderDetail> {
                    new OrderDetail("Prd1", "ABC", 4),
                    new OrderDetail("Prd5", "QWE", 23)
                }
            ),
            new Order(2, "Ord2", 750.42f, new DateTime(2014, 3, 30, 23, 45, 1), new Customer("Cus9"),
                new List<OrderDetail> {
                    new OrderDetail("Prd3", "FGH", 5),
                    new OrderDetail("Prd8", "QWE", 1),
                    new OrderDetail("Prd9", "QWE", 36)
                }
            ),
            new Order(3, "Ord3", 1125, new DateTime(2012, 11, 10, 8, 10, 25), new Customer("Cus3"),
                new List<OrderDetail> {
                    new OrderDetail("Prd2", "FGH", 63),
                    new OrderDetail("Prd4", "TYU", 5),
                    new OrderDetail("Prd6", "FGH", 18),
                    new OrderDetail("Prd9", "ABC", 22)
                }
            ),
            new Order(4, "Ord4", 231.58f, new DateTime(2011, 5, 1), new Customer("Cus1"),
                new List<OrderDetail> {
                    new OrderDetail("Prd7", "TYU", 4)
                }
            ),
            new Order(5, "Ord5", 1125, new DateTime(2010, 1, 28, 14, 42, 33), new Customer("Cus3"),
                new List<OrderDetail> {
                    new OrderDetail("Prd1", "QWE", 4),
                    new OrderDetail("Prd5", "BNM", 67),
                    new OrderDetail("Prd6", "BNM", 13),
                    new OrderDetail("Prd7", "TYU", 8),
                    new OrderDetail("Prd8", "FGH", 34),
                    new OrderDetail("Prd9", "FGH", 86)
                }
            )
        };

        internal static List<Product> Products = new List<Product> {
            new Product("Prd1", "Product 01", "Cat01"),
            new Product("Prd2", "Product 02", "Cat01"),
            new Product("Prd3", "Product 03", "Cat01"),
            new Product("Prd4", "Product 04", "Cat02"),
            new Product("Prd5", "Product 05", "Cat02"),
            new Product("Prd6", "Product 06", "Cat02"),
            new Product("Prd7", "Product 07", "Cat03"),
            new Product("Prd8", "Product 08", "Cat03"),
            new Product("Prd9", "Product 09", "Cat03")
        };
    }
}
