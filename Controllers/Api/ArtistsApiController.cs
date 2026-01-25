using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuseumSystem.Models;
using System.Security.Claims;

namespace MuseumSystem.Controllers.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ArtistsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ArtistsApiController> _logger;

        public ArtistsApiController(ApplicationDbContext context, ILogger<ArtistsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetArtists() => Ok(await _context.Artists.ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArtist(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            return artist == null ? NotFound() : Ok(artist);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Artist artist)
        {
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
            LogAction("CREATE", artist.Id);
            return CreatedAtAction(nameof(GetArtist), new { id = artist.Id }, artist);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Artist artist)
        {
            if (id != artist.Id) return BadRequest();
            _context.Entry(artist).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            LogAction("UPDATE", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null) return NotFound();
            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();
            LogAction("DELETE", id);
            return Ok(new { message = "Artist Deleted" });
        }

        private void LogAction(string action, int entityId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning("BONUS LOG | User: {UserId} | Action: {Action} | Artist ID: {Id} | Time: {Time}", 
                userId, action, entityId, DateTime.UtcNow);
        }
    }
}