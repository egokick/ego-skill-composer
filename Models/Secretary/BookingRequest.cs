namespace skill_composer.Models
{
    /// <summary>
    /// Booking Request.
    /// </summary>
    public class BookingRequest : BaseBooking
    { 
        public string CustomerEmail{ get; set; }
 
        public string CustomerFirstName { get; set; }

         
        public string CustomerMiddleName { get; set; }
         
        public string CustomerLastName { get; set; }
    }
}
