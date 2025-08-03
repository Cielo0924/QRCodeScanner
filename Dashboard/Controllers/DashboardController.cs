using Dashboard.Data;
using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalDispensers = _context.Dispensers.Count(),
                TotalLogs = _context.DispenserLogs.Count(),
                TotalRefills = _context.DispenserLogs.Count(l => l.ActionTaken == "Refill"),
                TotalChecks = _context.DispenserLogs.Count(l => l.ActionTaken == "Check"),
                TotalReplacements = _context.DispenserLogs.Count(l => l.ActionTaken == "Replace"),
                RecentLogs = _context.DispenserLogs
                    .Include(l => l.Dispenser)
                    .OrderByDescending(l => l.DateTime)
                    .Take(10)
                    .ToList(),

                // ✅ Include all dispensers
                Dispensers = _context.Dispensers.Include(d => d.Unit).ToList()
            };

            return View(viewModel);
        }


    }

}
