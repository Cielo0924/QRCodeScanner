using Dashboard.Data;
using Dashboard.Document;
using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;

namespace Dashboard.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Ibalik mo ito kung na-delete mo
        public IActionResult GenerateQRCodePDF(string id)
        {
            var dispenser = _context.Dispensers
                .Include(d => d.Unit)
                .FirstOrDefault(d => d.DispenserID == id);

            if (dispenser == null)
                return NotFound();

            // Generate QR code image
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(dispenser.QRCodeURL, QRCodeGenerator.ECCLevel.Q);
            var pngQrCode = new PngByteQRCode(qrCodeData);
            byte[] qrImage = pngQrCode.GetGraphic(20);

            // Generate PDF
            var doc = new QRDispenserDocument(
                dispenser.DispenserID,
                dispenser.Location,
                dispenser.Unit?.UnitID.ToString() ?? "N/A",
                dispenser.QRCodeImageBase64
            );

            var stream = new MemoryStream();
            doc.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", $"QR_{dispenser.DispenserID}.pdf");
        }



        public IActionResult Index(string? unitId, int unitPage = 1)
        {
            int pageSize = 10;

            var dispensersQuery = _context.Dispensers.Include(d => d.Unit).AsQueryable();

            if (!string.IsNullOrEmpty(unitId) && int.TryParse(unitId, out int parsedUnitId))
            {
                dispensersQuery = dispensersQuery.Where(d => d.UnitID == parsedUnitId);
                ViewBag.FilteredUnitID = parsedUnitId;
                ViewBag.FilteredUnitName = _context.Units.FirstOrDefault(u => u.UnitID == parsedUnitId)?.UnitName;
            }

            // Units for pagination
            var allUnits = _context.Units.OrderBy(u => u.UnitName).ToList();
            var pagedUnits = allUnits.Skip((unitPage - 1) * pageSize).Take(pageSize).ToList();
            int totalPages = (int)Math.Ceiling(allUnits.Count / (double)pageSize);

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
                Dispensers = dispensersQuery.ToList(),
                Units = pagedUnits
            };

            ViewBag.CurrentPage = unitPage;
            ViewBag.TotalPages = totalPages;

            return View(viewModel);
        }

        public IActionResult DispensersList(int unitId)
        {
            var unit = _context.Units.FirstOrDefault(u => u.UnitID == unitId);
            var dispensers = _context.Dispensers
                .Where(d => d.UnitID == unitId)
                .Include(d => d.Unit)
                .ToList();

            var viewModel = new DashboardViewModel
            {
                Dispensers = dispensers,
                Units = _context.Units.ToList(), // Optional: if you still need Units
            };

            ViewBag.FilteredUnitName = unit?.UnitName;
            ViewBag.FilteredUnitID = unit?.UnitID;

            return View(viewModel); // 👈 View file must be named `DispensersList.cshtml`
        }


    }
}
