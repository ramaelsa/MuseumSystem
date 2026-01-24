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

        // GET: Artists that Everyone can see
        public async Task<IActionResult> Index()
        {
            return View(await _context.Artists.ToListAsync());
        }

        // GET artists
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var artist = await _context.Artists
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (artist == null) return NotFound();

            return View(artist);
        }

        // --- ADMIN ONLY ACTIONS ---

        // GET: Artists/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Artists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,FullName,Bio")] Artist artist)
        {
            ModelState.Remove("Exhibits");

            if (ModelState.IsValid)
            {
                _context.Add(artist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        // GET: Artists
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var artist = await _context.Artists.FindAsync(id);
            if (artist == null) return NotFound();
            
            return View(artist);
        }

        // POST: Artists
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Bio")] Artist artist)
        {
            if (id != artist.Id) return NotFound();

            ModelState.Remove("Exhibits");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(artist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Artists.Any(e => e.Id == artist.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        // GET: Artists
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var artist = await _context.Artists
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (artist == null) return NotFound();

            return View(artist);
        }

        // POST: Artists
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist != null)
            {
                _context.Artists.Remove(artist);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}