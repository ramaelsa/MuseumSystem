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
    [Route("api/[controller]")]
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
        public async Task<IActionResult> GetExhibits(string searchString, string sortOrder, bool? isActive, int pageNumber = 1)
        {
            var query = _context.Exhibits.Include(e => e.Artist).AsQueryable();

            // Logic for Public Users 
            if (!User.Identity.IsAuthenticated)
            {
                query = query.Where(e => e.IsActive == true);
            }

            //  Filtering
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(e => e.Name.Contains(searchString) || e.Description.Contains(searchString));
            }

            if (isActive.HasValue)
            {
                query = query.Where(e => e.IsActive == isActive.Value);
            }

            // Ordering
            ViewData["CurrentSort"] = sortOrder;
            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(e => e.Name);
                    break;
                case "id_asc":
                    query = query.OrderBy(e => e.Id);
                    break;
                default:
                    query = query.OrderBy(e => e.Name);
                    break;
            }

            //  Pagination 
            int pageSize = 5;
            var totalItems = await query.CountAsync();
            var results = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Store metadata for MVC Views
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewData["CurrentPage"] = pageNumber;
            ViewData["SearchString"] = searchString;

            if (Request.Headers["Accept"].ToString().Contains("text/html"))
                return View("Index", results);

            return Ok(new
            {
                totalItems,
                pageNumber,
                pageSize,
                totalPages = ViewData["TotalPages"],
                data = results
            });
        }

        // GET: api/Exhibits
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
        [Authorize(Roles = "Admin")]
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

        // PUT: api/Exhibits
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
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

        // DELETE: api/Exhibits
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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
        
        [HttpGet("Create")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Create()
        {
            ViewData["ArtistId"] = new SelectList(_context.Artists, "Id", "FullName");
            return View();
        }
    }
}