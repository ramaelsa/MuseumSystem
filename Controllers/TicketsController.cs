using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuseumSystem.Models;
using System.Security.Claims;

namespace MuseumSystem.Controllers
{
    [Route("api/[controller]")]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<Ticket> tickets;

            if (User.IsInRole("Admin"))
            {
                tickets = await _context.Tickets.ToListAsync();
            }
            else if (User.Identity.IsAuthenticated)
            {
                tickets = await _context.Tickets
                    .Where(t => t.UserId == userId)
                    .ToListAsync();
            }
            else
            {
                tickets = new List<Ticket>();
            }

            if (Request.Headers["Accept"].ToString().Contains("text/html"))
                return View(tickets);

            return Ok(tickets);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            if (!User.IsInRole("Admin") && ticket.UserId != userId)
                return Forbid();

            if (Request.Headers["Accept"].ToString().Contains("text/html"))
                return View(ticket);

            return Ok(ticket);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] Ticket ticket)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ticket.UserId = userId;

            bool alreadyHasTicket = await _context.Tickets
                .AnyAsync(t => t.UserId == userId && t.VisitDate.Date == ticket.VisitDate.Date);

            if (alreadyHasTicket)
            {
                ModelState.AddModelError("VisitDate", "You already have a reservation for this date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    Action = "Create",
                    EntityName = "Ticket",
                    DateTime = DateTime.Now,
                    Details = $"Ticket created for {ticket.VisitorName} on {ticket.VisitDate.ToShortDateString()}"
                });

                await _context.SaveChangesAsync();

                if (Request.Headers["Accept"].ToString().Contains("text/html"))
                    return RedirectToAction(nameof(Index));

                return CreatedAtAction(nameof(Details), new { id = ticket.Id }, ticket);
            }

            return BadRequest(ModelState);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [FromBody] Ticket ticket)
        {
            if (id != ticket.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        Action = "Edit",
                        EntityName = "Ticket",
                        DateTime = DateTime.Now,
                        Details = $"Admin updated Ticket ID {id}"
                    });
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tickets.Any(e => e.Id == ticket.Id)) return NotFound();
                    else throw;
                }
                return NoContent();
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            if (!User.IsInRole("Admin") && ticket.UserId != userId)
                return Forbid();

            string actionType = User.IsInRole("Admin") ? "Delete" : "Cancel";

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = actionType,
                EntityName = "Ticket",
                DateTime = DateTime.Now,
                Details = $"{actionType} action on Ticket ID {id}"
            });

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("Create")]
        [Authorize]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Create() => View();

        [HttpGet("Edit/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.Tickets.FindAsync(id);
            return ticket == null ? NotFound() : View(ticket);
        }

        [HttpGet("Delete/{id}")]
        [Authorize(Roles = "Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.Tickets.FirstOrDefaultAsync(m => m.Id == id);
            return ticket == null ? NotFound() : View(ticket);
        }
    }
}