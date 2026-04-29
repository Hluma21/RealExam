namespace Greenfield.Models
{
    public class Basket //Used to define something that is inside the brackets. It can be accessible throughout the database
    {
        public int BasketId { get; set; } //A unique ID that represents that specific basket
        public bool Status { get; set; } //The order process 
        public string UserId { get; set; } //Links each basket to the user

        //Navigation Property
        public ICollection<BasketProduct>? BasketProducts { get; set; } //The basket can have a list of BasketProduct items. However it might also have none
    }
}
