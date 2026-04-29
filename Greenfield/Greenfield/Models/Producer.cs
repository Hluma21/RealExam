namespace Greenfield.Models
{
    public class Producer
    {
        public int ProducerId { get; set; } //A unique ID that represents that specific producer
        public string UserId { get; set; } //A unique ID that represents that specific user
        public string ProducerName { get; set; } //Provides the producer name
        public string ProducerEmail { get; set; } //provides the producer email
        public string ProducerInformation { get; set; } //provides the producer information

        //Navigation Property
        public ICollection<Product>? Products { get; set; } //This means producers can have multiple products, not just one.
    }
}
