namespace skill_composer.Models
{
    /// <summary>
    /// Represents a booking for a service in the booking application.
    /// </summary>
    public class BaseBooking
    {
       
        public int BookingId { get; set; }

    
        public int BusinessId { get; set; }

      
        public int CustomerId { get; set; }

       
        public int? ResourceId { get; set; }

     
        public int? EmployeeId { get; set; }

       
        public int? ServiceId { get; set; }

      
        public DateTime BookingDate { get; set; }

       
        public DateTime StartDateTime { get; set; }

     
        public DateTime? EndDateTime { get; set; }

       
        public int StatusId { get; set; }

      
        public string StatusInformation { get; set; }
         
        public string SchedulerJobId { get; set; }
    }
}
