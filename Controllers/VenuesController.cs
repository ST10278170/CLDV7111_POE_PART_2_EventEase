using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CLDV7111_POE_PART_1_EventEase.Data;
using CLDV7111_POE_PART_1_EventEase.Models;

namespace CLDV7111_POE_PART_1_EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenuesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Venues
        public async Task<IActionResult> Index(string search, string sortOrder, int page = 1)
        {
            int pageSize = 5;

            ViewBag.NameSort = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.CapacitySort = sortOrder == "cap_asc" ? "cap_desc" : "cap_asc";

            var venues = _context.Venues.AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                venues = venues.Where(v =>
                    v.VenueName.Contains(search) ||
                    v.VenueId.ToString() == search
                );
            }

            // SORTING
            venues = sortOrder switch
            {
                "name_asc" => venues.OrderBy(v => v.VenueName),
                "name_desc" => venues.OrderByDescending(v => v.VenueName),

                "cap_asc" => venues.OrderBy(v => v.Capacity),
                "cap_desc" => venues.OrderByDescending(v => v.Capacity),

                _ => venues.OrderBy(v => v.VenueId)
            };

            // PAGINATION
            int totalRecords = await venues.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;

            var paginated = await venues
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(paginated);
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
                return NotFound();

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue, IFormFile ImageFile)
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

                    venue.ImageUrl = "/images/" + uniqueFileName;
                }

                _context.Add(venue);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var venue = await _context.Venues.FindAsync(id);

            if (venue == null)
                return NotFound();

            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue, IFormFile ImageFile)
        {
            if (id != venue.VenueId)
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

                        venue.ImageUrl = "/images/" + uniqueFileName;
                    }
                    else
                    {
                        // KEEP OLD IMAGE
                        venue.ImageUrl = _context.Venues
                            .AsNoTracking()
                            .Where(v => v.VenueId == id)
                            .Select(v => v.ImageUrl)
                            .FirstOrDefault();
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var venue = await _context.Venues
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
                return NotFound();

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues.FindAsync(id);

            if (venue == null)
                return NotFound();

            // Restrict deletion if venue has active bookings
            bool hasBookings = _context.Bookings.Any(b => b.VenueId == id);

            if (hasBookings)
            {
                TempData["Error"] = "This venue has active bookings and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venues.Any(e => e.VenueId == id);
        }
    }
}
