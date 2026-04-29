namespace Greenfield.Models
{
    public class Order //Used to define something that is inside the brackets. It can be accessible throughout the database
    {
        public int OrderId { get; set; }  //A unique ID that represents that specific order
        public string UserId { get; set; } //Links the order to a user
        public float Subtotal { get; set; } //Price of all products added together
        public string? OrderType { get; set; } //This has to be nullable as they might pick collection
        public bool Collection {  get; set; } //Able to go and pick it up yourslef
        public bool Delivery { get; set; } //The store brings the order to you
        public string OrderStatus { get; set; } //Tells you what is happening right now
        public DateOnly? CollectionDate {  get; set; } //This has to be nullable as tehy might pcik collection
        public DateOnly OrderDate { get; set; } //What date will the order be delivered to you or collected

        //Navigation Property
        public ICollection<OrderProduct>? OrderProducts { get; set; } //An order can have a list of order items. However it might also have none
        
    }
}
