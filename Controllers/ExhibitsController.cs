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
    [Route("api/[controller]")] // Tells Swagger how to find the API
    public class ExhibitsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExhibitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Exhibits
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetExhibits()
        {
            var query = _context.Exhibits.Include(e => e.Artist).AsQueryable();

            if (!User.Identity.IsAuthenticated)
            {
                query = query.Where(e => e.IsActive == true);
            }

            var results = await query.ToListAsync();
            
            // If the request comes from a browser (MVC), return View. If from API, return Data.
            if (Request.Headers["Accept"].ToString().Contains("text/html"))
                return View("Index", results);
                
            return Ok(results);
        }

        // GET
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExhibit(int id)
        {
            var exhibit = await _context.Exhibits
                .Include(e => e.Artist)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (exhibit == null) return NotFound();

            if (Request.Headers["Accept"].ToString().Contains("text/html"))
                return View("Details", exhibit);

            return Ok(exhibit);
        }

        // POST: api/Exhibits
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateExhibit([FromBody] Exhibit exhibit)
        {
            ModelState.Remove("Artist");
            if (ModelState.IsValid)
            {
                _context.Add(exhibit);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetExhibit), new { id = exhibit.Id }, exhibit);
            }
            return BadRequest(ModelState);
        }

        // PUT
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateExhibit(int id, [FromBody] Exhibit exhibit)
        {
            if (id != exhibit.Id) return BadRequest();

            _context.Entry(exhibit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExhibitExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteExhibit(int id)
        {
            var exhibit = await _context.Exhibits.FindAsync(id);
            if (exhibit == null) return NotFound();

            _context.Exhibits.Remove(exhibit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExhibitExists(int id)
        {
            return _context.Exhibits.Any(e => e.Id == id);
        }
    }
}