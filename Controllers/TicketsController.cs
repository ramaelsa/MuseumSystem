using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuseumSystem.Models;
using System.Security.Claims;

namespace MuseumSystem.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous] 
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Admin"))
            {
                return View(await _context.Tickets.ToListAsync());
            }

            if (User.Identity.IsAuthenticated)
            {
                var myTickets = await _context.Tickets
                    .Where(t => t.UserId == userId)
                    .ToListAsync();
                return View(myTickets);
            }

            return View(new List<Ticket>());
        }

        [Authorize]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VisitorName,VisitDate,Price")] Ticket ticket)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ticket.UserId = userId;

            bool alreadyHasTicket = await _context.Tickets
                .AnyAsync(t => t.UserId == userId && t.VisitDate.Date == ticket.VisitDate.Date);

            if (alreadyHasTicket)
            {
                ModelState.AddModelError("VisitDate", "You already have a reservation for this date. Only one ticket per day is allowed.");
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
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,VisitorName,VisitDate,Price")] Ticket ticket)
        {
            if (id != ticket.Id) return NotFound();

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
                        Details = $"Admin updated Ticket ID {id} (Visitor: {ticket.VisitorName})"
                    });

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tickets.Any(e => e.Id == ticket.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.Tickets.FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Action = "Delete",
                    EntityName = "Ticket",
                    DateTime = DateTime.Now,
                    Details = $"Admin deleted/voided Ticket ID {id} for {ticket.VisitorName}"
                });

                _context.Tickets.Remove(ticket);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (ticket != null)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    Action = "Cancel",
                    EntityName = "Ticket",
                    DateTime = DateTime.Now,
                    Details = $"User cancelled their own Ticket ID {id}"
                });

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}