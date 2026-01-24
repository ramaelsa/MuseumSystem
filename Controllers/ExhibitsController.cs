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

        // GET: Exhibits
        [AllowAnonymous] // what visitors can access
        public async Task<IActionResult> Index()
        {
            var query = _context.Exhibits.Include(e => e.Artist).AsQueryable();

            // Public users can only view active entities
            if (!User.Identity.IsAuthenticated)
            {
                query = query.Where(e => e.IsActive == true);
            }

            return View(await query.ToListAsync());
        }

        // GET
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var exhibit = await _context.Exhibits
                .Include(e => e.Artist)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (exhibit == null) return NotFound();

            return View(exhibit);
        }

        // GET
        [Authorize] // Requires Login
        public IActionResult Create()
        {
            ViewData["ArtistId"] = new SelectList(_context.Artists, "Id", "FullName");
            return View();
        }

        // POST
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IsActive,ArtistId")] Exhibit exhibit)
        {
            ModelState.Remove("Artist");

            if (ModelState.IsValid)
            {
                _context.Add(exhibit);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ArtistId"] = new SelectList(_context.Artists, "Id", "FullName", exhibit.ArtistId);
            return View(exhibit);
        }

        // GET
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var exhibit = await _context.Exhibits.FindAsync(id);
            if (exhibit == null) return NotFound();
            
            ViewData["ArtistId"] = new SelectList(_context.Artists, "Id", "FullName", exhibit.ArtistId);
            return View(exhibit);
        }

        // POST
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive,ArtistId")] Exhibit exhibit)
        {
            if (id != exhibit.Id) return NotFound();

            ModelState.Remove("Artist");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(exhibit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExhibitExists(exhibit.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ArtistId"] = new SelectList(_context.Artists, "Id", "FullName", exhibit.ArtistId);
            return View(exhibit);
        }

        // GET
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var exhibit = await _context.Exhibits
                .Include(e => e.Artist)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (exhibit == null) return NotFound();

            return View(exhibit);
        }

        // POST
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var exhibit = await _context.Exhibits.FindAsync(id);
            if (exhibit != null)
            {
                _context.Exhibits.Remove(exhibit);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ExhibitExists(int id)
        {
            return _context.Exhibits.Any(e => e.Id == id);
        }
    }
}