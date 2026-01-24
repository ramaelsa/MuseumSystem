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

        public ExhibitsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetExhibits()
        {
            var exhibits = await _context.Exhibits
                .Include(e => e.Artist)
                .Select(e => new 
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.IsActive,
                    ArtistName = e.Artist != null ? e.Artist.FullName : "Unknown Artist"
                })
                .ToListAsync();

            return Ok(exhibits);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExhibit(int id)
        {
            var exhibit = await _context.Exhibits
                .Include(e => e.Artist)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exhibit == null)
            {
                return NotFound(new { message = $"Exhibit {id} not found." });
            }

            return Ok(new
            {
                exhibit.Id,
                exhibit.Name,
                exhibit.Description,
                exhibit.IsActive,
                ArtistName = exhibit.Artist != null ? exhibit.Artist.FullName : "Unknown Artist"
            });
        }
    }
}