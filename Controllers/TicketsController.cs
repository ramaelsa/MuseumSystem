using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuseumSystem.Models;

namespace MuseumSystem.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tickets
        [AllowAnonymous] 
        public async Task<IActionResult> Index()
        {
            // If Admin, show the full database of sales
            if (User.IsInRole("Admin"))
            {
                return View(await _context.Tickets.ToListAsync());
            }

            // If Visitor/User, just show the info page
            return View(new List<Ticket>());
        }

        // GET: Tickets/Create
        [Authorize] // Only logged-in users can reach the booking form
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VisitorName,VisitDate,Price")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = ticket.Id });
            }
            return View(ticket);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.Tickets.FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null) return NotFound();
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,VisitorName,VisitDate,Price")] Ticket ticket)
        {
            if (id != ticket.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
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
            if (ticket != null) _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}