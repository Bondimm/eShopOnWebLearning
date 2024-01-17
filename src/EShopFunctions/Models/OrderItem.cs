namespace EShopFunctions.Models
{
    public class OrderItem
    {
        public CatalogItemOrdered itemOrdered { get; set; }
        public decimal unitPrice { get; set; }
        public int units { get; set; }
    }
}
