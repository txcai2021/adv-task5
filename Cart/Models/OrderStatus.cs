using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CartsApi.Models
{
    public class OrderStatus
    {
        public int OrderId { get; set; }
        public int CartId { get; set; }

        public string Status { get; set; }

    }
}
