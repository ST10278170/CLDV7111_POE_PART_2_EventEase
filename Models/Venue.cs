namespace CLDV7111_POE_PART_1_EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        public string? VenueName { get; set; }
        public string? Location { get; set; }
        public int Capacity { get; set; }
        public string? ImageUrl { get; set; }

        // Navigation
        public ICollection<Event>? Events { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
    }
}


