namespace CLDV7111_POE_PART_1_EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }

        public string? EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }


        // Foreign Key (Event MUST belong to a Venue)
        public int VenueId { get; set; }

        // Navigation
        public Venue? Venue { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
    }
}
