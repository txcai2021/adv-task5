using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrdersApi.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int CartId { get; set; }
      
        public List<OrderDetail> Details { get; set; }
        public double Total { get; set; }

        public string CustomerId { get; set; }

        public string Status { get; set; }

    }

    public class OrderDetail
    {
        public int ProductId { get; set; }
        public double Price { get; set; }

        public int Quantity { get; set; }
    }

    public enum EOrderStatus : byte
    {
        INITIATED = 0,
        SUCCESS = 10,
        FAILED = 20      
    }

}
