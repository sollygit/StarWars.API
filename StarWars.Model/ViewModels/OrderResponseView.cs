using System;
using System.Collections.Generic;

namespace StarWars.Model.ViewModels
{
    public class OrderResponseView
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public IEnumerable<Order> Items { get; set; } = Array.Empty<Order>();
    }
}
