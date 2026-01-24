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
    [Route("api/[controller]")] // This makes it show up in Swagger
    public class ArtistsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArtistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Artists
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var artists = await _context.Artists.ToListAsync();
            
            // Check if request is from browser or API/Swagger
            if (Request.Headers["Accept"].ToString().Contains("text/html"))
                return View(artists);

            return Ok(artists);
        }

        // GET: api/Artists/5
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

        // --- ADMIN ONLY ACTIONS ---

        // POST: api/Artists
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

        // PUT: api/Artists
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

        // DELETE: api/Artists
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