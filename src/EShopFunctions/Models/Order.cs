using System.Collections.Generic;

namespace EShopFunctions.Models
{
    public class Order
    {
        public string id { get; set; }

        public int orderId { get; set; }

        public Address shipToAddress { get; set; }

        public List<OrderItem> orderItems { get; set; }

        public decimal finalPrice { get; set; }
    }
}
