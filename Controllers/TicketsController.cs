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

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = _context.Tickets.AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(t => t.UserId == userId);
            }

            return View(await query.ToListAsync());
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
        public async Task<IActionResult> Edit(int id, string reasonForChange, [Bind("Id,VisitorName,VisitDate")] Ticket ticket)
        {
            if (id != ticket.Id) return NotFound();

            var originalTicket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (originalTicket == null) return NotFound();

            ticket.Price = originalTicket.Price;
            ticket.UserId = originalTicket.UserId;

            ModelState.Remove("UserId");
            ModelState.Remove("Price");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    _context.AuditLogs.Add(new AuditLog
                    {
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        Action = "Admin Correction",
                        EntityName = "Ticket",
                        DateTime = DateTime.Now,
                        Details = $"Ticket {id} modified. Reason: {reasonForChange}. Price maintained at {originalTicket.Price}."
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

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var priceSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "TicketPrice");
            ViewBag.CurrentPrice = priceSetting?.Value ?? 25.00m;
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var priceSetting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "TicketPrice");
            
            ticket.UserId = userId;
            ticket.Price = priceSetting?.Value ?? 25.00m;

            ModelState.Remove("UserId");
            ModelState.Remove("Price");

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CurrentPrice = ticket.Price;
            return View(ticket);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetPrice()
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "TicketPrice");
            return View(setting);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrice(decimal newValue)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == "TicketPrice");
            if (setting != null)
            {
                setting.Value = newValue;
                _context.Update(setting);
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Action = "Price Update",
                    EntityName = "SystemSetting",
                    DateTime = DateTime.Now,
                    Details = $"Global price set to {newValue}."
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.Tickets.FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null) return NotFound();
            if (!User.IsInRole("Admin") && ticket.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();
            return View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}