using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuseumSystem.Models;

namespace MuseumSystem.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Admin"))
        {
            ViewBag.TotalTickets = await _context.Tickets.CountAsync();
            ViewBag.TotalRevenue = await _context.Tickets.SumAsync(t => t.Price);
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.RecentLogs = await _context.AuditLogs
                .OrderByDescending(l => l.DateTime)
                .Take(5)
                .ToListAsync();
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}