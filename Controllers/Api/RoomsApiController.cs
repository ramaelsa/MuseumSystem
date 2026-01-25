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
    public class RoomsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomsApiController> _logger;

        public RoomsApiController(ApplicationDbContext context, ILogger<RoomsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms() => Ok(await _context.Rooms.ToListAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            LogAction("CREATE", room.Id);
            return Ok(room);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Room room)
        {
            if (id != room.Id) return BadRequest();
            _context.Entry(room).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            LogAction("UPDATE", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            LogAction("DELETE", id);
            return Ok(new { message = "Room Deleted" });
        }

        private void LogAction(string action, int entityId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogWarning("BONUS LOG | User: {UserId} | Action: {Action} | Room ID: {Id} | Time: {Time}", 
                userId, action, entityId, DateTime.UtcNow);
        }
    }
}