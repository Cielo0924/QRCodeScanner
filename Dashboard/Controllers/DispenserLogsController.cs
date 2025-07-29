using Dashboard.Data;
using Dashboard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Net;

namespace Dashboard.Controllers
{
    public class DispenserLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DispenserLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(string dispenserId)
        {
            ViewBag.Dispensers = _context.Dispensers
                .Select(d => new SelectListItem
                {
                    Value = d.DispenserID,
                    Text = d.DispenserID + " (" + d.Location + ")"
                })
                .ToList();

            var log = new DispenserLog();
            if (!string.IsNullOrEmpty(dispenserId))
                log.DispenserID = dispenserId;

            return View(log);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DispenserLog log)
        {
            if (ModelState.IsValid)
            {
                log.DateTime = DateTime.Now;
                _context.DispenserLogs.Add(log);
                _context.SaveChanges();

                // ✅ Save DispenserID to session
                HttpContext.Session.SetString("GeneratedDispenserID", log.DispenserID);

                // 🔥 Get IP address of local machine
                var ip = Dns.GetHostEntry(Dns.GetHostName())
                            .AddressList
                            .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                            .ToString();

                // ⚡ Use IP instead of localhost for QR code
                string url = $"https://{ip}:{Request.Host.Port}/DispenserLogs/LogForm?dispenserId={log.DispenserID}";

                // 🎯 QR code generation
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new BitmapByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                string base64Image = Convert.ToBase64String(qrCodeImage);

                HttpContext.Session.SetString("QRCodeImage", base64Image);
                return RedirectToAction("QRGenerated");
            }

            return View(log);
        }


        [HttpGet]
        public IActionResult Edit(string dispenserId)
        {
            if (string.IsNullOrEmpty(dispenserId))
                return NotFound();

            var log = _context.DispenserLogs.FirstOrDefault(l => l.DispenserID == dispenserId);
            if (log == null)
                return NotFound();

            ViewBag.Dispensers = _context.Dispensers
                .Select(d => new SelectListItem
                {
                    Value = d.DispenserID,
                    Text = d.DispenserID + " (" + d.Location + ")"
                })
                .ToList();

            return View("Create", log); // reuse the Create view for editing
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DispenserLog log)
        {
            if (ModelState.IsValid)
            {
                _context.DispenserLogs.Update(log);
                _context.SaveChanges();
                return RedirectToAction("QRGenerated");
            }

            return View("Create", log);
        }

        // GET: show the QR code
        [HttpGet]
        public IActionResult QRGenerated()
        {
            var qrBase64 = HttpContext.Session.GetString("QRCodeImage");
            var dispenserId = HttpContext.Session.GetString("GeneratedDispenserID");

            if (string.IsNullOrEmpty(qrBase64) || string.IsNullOrEmpty(dispenserId))
            {
                TempData["Error"] = "QR Code generation failed or session expired.";
                return RedirectToAction("Create");
            }

            // Kunin ang dispenser info from DB (example using EF Core)
            var dispenser = _context.Dispensers.FirstOrDefault(d => d.DispenserID == dispenserId);

            ViewBag.Dispenser = dispenser; // Ito ang ilalagay para ma-access sa view

            return View();
        }



        // POST: create new dispenser and generate QR
        [HttpPost]
        public async Task<IActionResult> QRGenerated(Dispenser dispenser)
        {
            if (_context.Dispensers.Any(d => d.DispenserID == dispenser.DispenserID))
            {
                TempData["Error"] = "Dispenser already exists.";
                return RedirectToAction("Create");
            }

            _context.Dispensers.Add(dispenser);
            await _context.SaveChangesAsync();

            string url = $"{Request.Scheme}://{Request.Host}/DispenserLogs/LogForm?dispenserId={dispenser.DispenserID}";
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new Base64QRCode(qrCodeData);
            string qrBase64 = qrCode.GetGraphic(20);

            HttpContext.Session.SetString("QRCodeImage", qrBase64);

            return RedirectToAction("QRGenerated");
        }



        [HttpGet]
        public IActionResult LogForm(string dispenserId)
        {
            if (string.IsNullOrEmpty(dispenserId))
                return NotFound("Dispenser ID is required.");

            var dispenser = _context.Dispensers.FirstOrDefault(d => d.DispenserID == dispenserId);
            if (dispenser == null)
                return NotFound("Dispenser not found.");

            var logs = _context.DispenserLogs
                .Where(l => l.DispenserID == dispenserId)
                .OrderByDescending(l => l.DateTime)
                .ToList();

            ViewBag.Dispenser = dispenser;
            return View(logs);
        }

    }
}
