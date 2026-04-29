namespace Greenfield.Models
{
    public class BasketProduct //Used to define something that is inside the brackets. It can be accessible throughout the database
    {
        public int BasketProductId { get; set; } //A unique ID that represents that specific basket product
        public int BasketId { get; set; } //Links to a basket
        public int ProductId { get; set; } //Links to a product
        public int Quaility { get; set; } //It was meant to be Quantity not Quality. However, it's shows the number of something

        //Navigation Property
        public Product Products { get; set; } //This links basket products to one product
        public Basket Basket { get; set; } //This links basket products to one basket

    }
}
