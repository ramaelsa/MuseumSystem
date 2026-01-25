using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuseumSystem.Models;

namespace MuseumSystem.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ExhibitsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExhibitsApiController> _logger;

        public ExhibitsApiController(ApplicationDbContext context, ILogger<ExhibitsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ExhibitsApi read
        [HttpGet]
        public async Task<IActionResult> GetExhibits()
        {
            var exhibits = await _context.Exhibits
                .Include(e => e.Artist)
                .Select(e => new 
                {
                    e.Id, e.Name, e.Description, e.IsActive,
                    ArtistName = e.Artist != null ? e.Artist.FullName : "Unknown Artist"
                }).ToListAsync();
            return Ok(exhibits);
        }

        // GET: api/ExhibitsApi/5 read one
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExhibit(int id)
        {
            var exhibit = await _context.Exhibits.FindAsync(id);
            if (exhibit == null) return NotFound();
            return Ok(exhibit);
        }

        // POST: api/ExhibitsApi create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateExhibit([FromBody] Exhibit exhibit)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Exhibits.Add(exhibit);
            await _context.SaveChangesAsync();

            LogAction("CREATE", exhibit.Id); // BONUS LOGGING
            return CreatedAtAction(nameof(GetExhibit), new { id = exhibit.Id }, exhibit);
        }

        // PUT: api/ExhibitsApi/5 update
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateExhibit(int id, [FromBody] Exhibit exhibit)
        {
            if (id != exhibit.Id) return BadRequest();

            _context.Entry(exhibit).State = EntityState.Modified;
            
            try {
                await _context.SaveChangesAsync();
                LogAction("UPDATE", id); // BONUS LOGGING
            }
            catch (DbUpdateConcurrencyException) {
                if (!_context.Exhibits.Any(e => e.Id == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/ExhibitsApi/5 delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteExhibit(int id)
        {
            var exhibit = await _context.Exhibits.FindAsync(id);
            if (exhibit == null) return NotFound();

            _context.Exhibits.Remove(exhibit);
            await _context.SaveChangesAsync();

            LogAction("DELETE", id); // BONUS LOGGING
            return Ok(new { message = $"Exhibit {id} deleted successfully." });
        }
        
        private void LogAction(string action, int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning("ACTION: {Action} | ENTITY: Exhibit | ID: {Id} | USER: {User} | TIME: {Time}", 
                action, id, userId ?? "Unknown", DateTime.UtcNow);
        }
    }
}