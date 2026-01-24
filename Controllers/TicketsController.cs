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

        // GET: api/Tickets
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, string sortOrder, int pageNumber = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Start the query based on who is asking
            var query = _context.Tickets.AsQueryable();

            if (User.IsInRole("Admin"))
            {
                // Admins see everything
            }
            else if (User.Identity.IsAuthenticated)
            {
                // Regular users only see their own stuff
                query = query.Where(t => t.UserId == userId);
            }
            else
            {
                // Not logged in? You get nothing.
                query = query.Where(t => false);
            }

            //  Search by Visitor Name (Criteria #3)
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.VisitorName.Contains(searchString));
            }

            // Sorting by Date 
            ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
            switch (sortOrder)
            {
                case "date_asc":
                    query = query.OrderBy(t => t.VisitDate);
                    break;
                default:
                    query = query.OrderByDescending(t => t.VisitDate);
                    break;
            }

            //  Pagination
            int pageSize = 10; // We can show more tickets per page than exhibits
            var totalItems = await query.CountAsync();
            var tickets = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass info to the frontend team
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewData["CurrentPage"] = pageNumber;

            if (Request.Headers["Accept"].ToString().Contains("text/html"))
                return View(tickets);

            return Ok(new
            {
                totalItems,
                totalPages = ViewData["TotalPages"],
                currentPage = pageNumber,
                data = tickets
            });
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            // Safety check: Don't let users peek at other people's tickets
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

            // One ticket per day rule 
            bool alreadyHasTicket = await _context.Tickets
                .AnyAsync(t => t.UserId == userId && t.VisitDate.Date == ticket.VisitDate.Date);

            if (alreadyHasTicket)
            {
                ModelState.AddModelError("VisitDate", "You already have a reservation for this date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                
                // Logging the action for the bonus points
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