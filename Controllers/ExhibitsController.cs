using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MuseumSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace MuseumSystem.Controllers
{
    public class ExhibitsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExhibitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string sortOrder, bool? isActive, int pageNumber = 1)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["SearchString"] = searchString;

            var query = _context.Exhibits.Include(e => e.Artist).Include(e => e.Room).AsQueryable();

            if (!User.Identity.IsAuthenticated) query = query.Where(e => e.IsActive);
            if (!string.IsNullOrEmpty(searchString)) query = query.Where(e => e.Name.Contains(searchString) || e.Artist.FullName.Contains(searchString));
            if (isActive.HasValue) query = query.Where(e => e.IsActive == isActive.Value);

            switch (sortOrder)
            {
                case "name_desc": query = query.OrderByDescending(e => e.Name); break;
                case "artist": query = query.OrderBy(e => e.Artist.FullName); break;
                case "id_desc": query = query.OrderByDescending(e => e.Id); break;
                default: query = query.OrderBy(e => e.Name); break;
            }

            int pageSize = 6;
            var totalItems = await query.CountAsync();
            var results = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewData["CurrentPage"] = pageNumber;

            return View(results);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.ArtistId = new SelectList(_context.Artists, "Id", "FullName");
            ViewBag.RoomId = new SelectList(_context.Rooms, "Id", "Name");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsActive,ImageUrl,ArtistId,RoomId")] Exhibit exhibit)
        {
            ModelState.Remove("Artist"); ModelState.Remove("Room");
            if (ModelState.IsValid) { _context.Add(exhibit); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
            ViewBag.ArtistId = new SelectList(_context.Artists, "Id", "FullName", exhibit.ArtistId);
            ViewBag.RoomId = new SelectList(_context.Rooms, "Id", "Name", exhibit.RoomId);
            return View(exhibit);
        }

        public async Task<IActionResult> Details(int id)
        {
            var exhibit = await _context.Exhibits.Include(e => e.Artist).Include(e => e.Room).FirstOrDefaultAsync(m => m.Id == id);
            return exhibit == null ? NotFound() : View(exhibit);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var exhibit = await _context.Exhibits.FindAsync(id);
            if (exhibit == null) return NotFound();
            ViewBag.ArtistId = new SelectList(_context.Artists, "Id", "FullName", exhibit.ArtistId);
            ViewBag.RoomId = new SelectList(_context.Rooms, "Id", "Name", exhibit.RoomId);
            return View(exhibit);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive,ImageUrl,ArtistId,RoomId")] Exhibit exhibit)
        {
            if (id != exhibit.Id) return BadRequest();
            ModelState.Remove("Artist"); ModelState.Remove("Room");
            if (ModelState.IsValid) { _context.Update(exhibit); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
            return View(exhibit);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var exhibit = await _context.Exhibits.Include(e => e.Artist).FirstOrDefaultAsync(m => m.Id == id);
            return exhibit == null ? NotFound() : View(exhibit);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exhibit = await _context.Exhibits.FindAsync(id);
            if (exhibit != null) { _context.Exhibits.Remove(exhibit); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}