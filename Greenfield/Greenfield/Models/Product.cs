namespace Greenfield.Models
{
    public class Product
    {
        public int ProductId { get; set; } //A unique ID that represents that specific product
        public int ProducerId { get; set; } //Links each product to a producer
        public string ProductName { get; set; } //Provides product name
        public string Description { get; set; } //Provides product desription 
        public float Rating { get; set; } //Provides product rating
        public int stock { get; set; } //Provides product stock
        public bool IsAvailable { get; set; } //Is the product available or unavailable
        public float Price { get; set; } //The price of the product
        public string? ImagePath { get; set; } //The image of the product

        //Navigation Property
        public Producer Producer { get; set; } //This links a product to one specific producer, not a list of producers
        public ICollection<OrderProduct>? OrderProducts { get; set; } //This means an order can have multiple products
        public ICollection<BasketProduct>? BasketProducts { get; set; } //This means a basket can have multiple products
    }
}
