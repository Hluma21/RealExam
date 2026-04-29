namespace Greenfield.Models
{
    public class OrderProduct
    {
        public int OrderProductId { get; set; } //A unique ID that represents that specific ordered product
        public int OrderId { get; set; } //A unique ID that represents that specific order
        public int ProductId { get; set; } //A unique ID that represents that specific prodcut
        public int Quantity { get; set; } //how many

        //Navigation Property
        public Product Products { get; set; } //This links ordered products to one product
        public Order Orders { get; set; } //This links ordered prodcts to one order
    }
}
