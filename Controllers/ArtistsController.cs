using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuseumSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace MuseumSystem.Controllers
{
    public class ArtistsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArtistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int pageNumber = 1)
        {
            var query = _context.Artists.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => a.FullName.Contains(searchString) || a.Bio.Contains(searchString));
            }

            ViewData["SearchString"] = searchString;

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
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,Bio,ImageUrl")] Artist artist)
        {
            if (ModelState.IsValid)
            {
                _context.Add(artist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null) return NotFound();
            return View(artist);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Bio,ImageUrl")] Artist artist)
        {
            if (id != artist.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(artist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtistExists(artist.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var artist = await _context.Artists
                .Include(a => a.Exhibits)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (artist == null) return NotFound();
            return View(artist);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var artist = await _context.Artists.FirstOrDefaultAsync(m => m.Id == id);
            if (artist == null) return NotFound();
            return View(artist);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist != null) _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArtistExists(int id)
        {
            return _context.Artists.Any(e => e.Id == id);
        }
    }
}