using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CLDV7111_POE_PART_1_EventEase.Data;
using CLDV7111_POE_PART_1_EventEase.Models;

namespace CLDV7111_POE_PART_1_EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(string search, string sortOrder, int page = 1)
        {
            int pageSize = 5;

            ViewBag.DateSort = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewBag.EventSort = sortOrder == "event_asc" ? "event_desc" : "event_asc";
            ViewBag.VenueSort = sortOrder == "venue_asc" ? "venue_desc" : "venue_asc";

            var bookings = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                bookings = bookings.Where(b =>
                    b.Event.EventName.Contains(search) ||
                    b.Venue.VenueName.Contains(search) ||
                    b.BookingId.ToString() == search
                );
            }

            // SORTING
            bookings = sortOrder switch
            {
                "date_asc" => bookings.OrderBy(b => b.BookingDate),
                "date_desc" => bookings.OrderByDescending(b => b.BookingDate),

                "event_asc" => bookings.OrderBy(b => b.Event.EventName),
                "event_desc" => bookings.OrderByDescending(b => b.Event.EventName),

                "venue_asc" => bookings.OrderBy(b => b.Venue.VenueName),
                "venue_desc" => bookings.OrderByDescending(b => b.Venue.VenueName),

                _ => bookings.OrderBy(b => b.BookingId)
            };

            // PAGINATION
            int totalRecords = await bookings.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;

            var paginated = await bookings
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(paginated);
        }

        // ⭐ CONSOLIDATED BOOKINGS VIEW
        public async Task<IActionResult> Consolidated(string search, string sortOrder, int page = 1)
        {
            int pageSize = 5;

            ViewBag.DateSort = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            ViewBag.EventSort = sortOrder == "event_asc" ? "event_desc" : "event_asc";
            ViewBag.VenueSort = sortOrder == "venue_asc" ? "venue_desc" : "venue_asc";

            var data = from b in _context.Bookings
                       join e in _context.Events on b.EventId equals e.EventId
                       join v in _context.Venues on b.VenueId equals v.VenueId
                       select new BookingDisplayVM
                       {
                           BookingId = b.BookingId,
                           EventName = e.EventName,
                           VenueName = v.VenueName,
                           BookingDate = b.BookingDate
                       };

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x =>
                    x.EventName.Contains(search) ||
                    x.VenueName.Contains(search) ||
                    x.BookingId.ToString() == search
                );
            }

            // SORTING
            data = sortOrder switch
            {
                "date_asc" => data.OrderBy(x => x.BookingDate),
                "date_desc" => data.OrderByDescending(x => x.BookingDate),

                "event_asc" => data.OrderBy(x => x.EventName),
                "event_desc" => data.OrderByDescending(x => x.EventName),

                "venue_asc" => data.OrderBy(x => x.VenueName),
                "venue_desc" => data.OrderByDescending(x => x.VenueName),

                _ => data.OrderBy(x => x.BookingId)
            };

            // PAGINATION
            int totalRecords = await data.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;

            var paginated = await data
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(paginated);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName");
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            // DOUBLE BOOKING VALIDATION
            bool exists = _context.Bookings.Any(b =>
                b.VenueId == booking.VenueId &&
                b.BookingDate.Date == booking.BookingDate.Date
            );

            if (exists)
            {
                ModelState.AddModelError(string.Empty, "This venue is already booked for the selected date.");

                ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);

                return View(booking);
            }

            _context.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);

            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.BookingId))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking != null)
                _context.Bookings.Remove(booking);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}
