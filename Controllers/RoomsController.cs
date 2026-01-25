using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuseumSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace MuseumSystem.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sortOrder, int page = 1)
        {
            ViewData["CurrentSort"] = sortOrder;
            var query = _context.Rooms.Include(r => r.Exhibits).AsQueryable();

            switch (sortOrder)
            {
                case "name_desc": query = query.OrderByDescending(r => r.Name); break;
                case "floor": query = query.OrderBy(r => r.Floor); break;
                case "floor_desc": query = query.OrderByDescending(r => r.Floor); break;
                default: query = query.OrderBy(r => r.Name); break;
            }

            int pageSize = 6;
            var totalItems = await query.CountAsync();
            var rooms = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(rooms);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Floor,ImageUrl")] Room room)
        {
            if (ModelState.IsValid) { _context.Add(room); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
            return View(room);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FindAsync(id);
            return room == null ? NotFound() : View(room);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Floor,ImageUrl")] Room room)
        {
            if (id != room.Id) return NotFound();
            if (ModelState.IsValid) { _context.Update(room); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
            return View(room);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.Include(r => r.Exhibits).ThenInclude(e => e.Artist).FirstOrDefaultAsync(m => m.Id == id);
            return room == null ? NotFound() : View(room);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var room = await _context.Rooms.FirstOrDefaultAsync(m => m.Id == id);
            return room == null ? NotFound() : View(room);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null) { _context.Rooms.Remove(room); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}