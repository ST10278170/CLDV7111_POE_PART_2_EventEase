using System;
using System.ComponentModel.DataAnnotations;

namespace CLDV7111_POE_PART_1_EventEase.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        // Booking Date
        [Required(ErrorMessage = "Booking date is required.")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "An event must be selected.")]
        public int EventId { get; set; }

        [Required(ErrorMessage = "A venue must be selected.")]
        public int VenueId { get; set; }

        // Navigation Properties
        public Event? Event { get; set; }
        public Venue? Venue { get; set; }
    }
}
