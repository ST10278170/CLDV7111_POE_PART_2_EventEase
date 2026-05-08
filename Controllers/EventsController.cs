using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CLDV7111_POE_PART_1_EventEase.Data;
using CLDV7111_POE_PART_1_EventEase.Models;

namespace CLDV7111_POE_PART_1_EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index(string search, string sortOrder, int page = 1)
        {
            int pageSize = 5;

            ViewBag.NameSort = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.DateSort = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            var events = _context.Events.Include(e => e.Venue).AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                events = events.Where(e =>
                    e.EventName.Contains(search) ||
                    e.EventId.ToString() == search
                );
            }

            // SORTING
            events = sortOrder switch
            {
                "name_asc" => events.OrderBy(e => e.EventName),
                "name_desc" => events.OrderByDescending(e => e.EventName),

                "date_asc" => events.OrderBy(e => e.EventDate),
                "date_desc" => events.OrderByDescending(e => e.EventDate),

                _ => events.OrderBy(e => e.EventId)
            };

            // PAGINATION
            int totalRecords = await events.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;

            var paginated = await events
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(paginated);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventItem == null)
                return NotFound();

            return View(eventItem);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                // LOCAL FILE STORAGE
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    eventItem.ImageUrl = "/images/" + uniqueFileName;
                }

                _context.Add(eventItem);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            return View(eventItem);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events.FindAsync(id);

            if (eventItem == null)
                return NotFound();

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            return View(eventItem);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventItem, IFormFile ImageFile)
        {
            if (id != eventItem.EventId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // LOCAL FILE STORAGE (Replace image if new one uploaded)
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        eventItem.ImageUrl = "/images/" + uniqueFileName;
                    }
                    else
                    {
                        // KEEP OLD IMAGE
                        eventItem.ImageUrl = _context.Events
                            .AsNoTracking()
                            .Where(e => e.EventId == id)
                            .Select(e => e.ImageUrl)
                            .FirstOrDefault();
                    }

                    _context.Update(eventItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventItem.EventId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", eventItem.VenueId);
            return View(eventItem);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventItem == null)
                return NotFound();

            return View(eventItem);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
                return NotFound();

            // Restrict deletion if event has active bookings
            bool hasBookings = _context.Bookings.Any(b => b.EventId == id);

            if (hasBookings)
            {
                TempData["Error"] = "This event has active bookings and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}
