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
    [Route("api/[controller]")]
    public class ArtistsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArtistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Artists (Includes search and pagination for the team)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pageNumber = 1)
        {
            // Start the query
            var query = _context.Artists.AsQueryable();

            // 1. Search by Name
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(a => a.FullName.Contains(searchString));
            }

            // 2. Sorting 
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(a => a.FullName);
                    break;
                default:
                    query = query.OrderBy(a => a.FullName);
                    break;
            }

            // 3. Pagination  - 5 items per page
            int pageSize = 5;
            var totalItems = await query.CountAsync();
            var artists = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Metadata for the frontend pages
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewData["CurrentPage"] = pageNumber;

            // If a browser asks for HTML, show the website view. Otherwise, send JSON data.
            if (Request.Headers["Accept"].ToString().Contains("text/html"))
                return View(artists);

            return Ok(new
            {
                totalItems,
                totalPages = ViewData["TotalPages"],
                currentPage = pageNumber,
                data = artists
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var artist = await _context.Artists
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (artist == null) return NotFound();

            if (Request.Headers["Accept"].ToString().Contains("text/html"))
                return View(artist);

            return Ok(artist);
        }

        // --- ADMIN ONLY ACTIONS (Need Token/Admin Login) ---

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Artist artist)
        {
            ModelState.Remove("Exhibits");

            if (ModelState.IsValid)
            {
                _context.Add(artist);
                await _context.SaveChangesAsync();
                
                if (Request.Headers["Accept"].ToString().Contains("text/html"))
                    return RedirectToAction(nameof(Index));

                return CreatedAtAction(nameof(Details), new { id = artist.Id }, artist);
            }
            return BadRequest(ModelState);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [FromBody] Artist artist)
        {
            if (id != artist.Id) return BadRequest();

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
                
                return NoContent();
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null) return NotFound();

            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        // These methods are just for the Razor Pages (Forms)
        [HttpGet("Create")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Create() => View();

        [HttpGet("Edit/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Edit(int? id)
        {
            var artist = await _context.Artists.FindAsync(id);
            return artist == null ? NotFound() : View(artist);
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Delete(int? id)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(m => m.Id == id);
            return artist == null ? NotFound() : View(artist);
        }
    }
}