using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EXAM.Data;
using EXAM.Models;
using EXAM.Services;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;

namespace EXAM.Controllers
{
    public class TicketsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly SqsService _sqsService;
        private readonly S3Service _s3Service;

        public TicketsController(AppDbContext db, SqsService sqsService, S3Service s3Service)
        {
            _db = db;
            _sqsService = sqsService;
            _s3Service = s3Service;
        }

        // GET: /Tickets
        public async Task<IActionResult> Index()
        {
            var items = await _db.Tickets.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return View(items);
        }

        // GET: /Tickets/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        // GET: /Tickets/Create
        public IActionResult Create()
        {
            return View(new Ticket());
        }

        // POST: /Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket model, Microsoft.AspNetCore.Http.IFormFile? attachment)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Tickets.Add(model);
            await _db.SaveChangesAsync();

            // upload attachment to S3 (optional)
            if (attachment != null && attachment.Length > 0)
            {
                var key = $"tickets/{model.Id}/{System.IO.Path.GetFileName(attachment.FileName)}";
                var url = await _s3Service.UploadFileAsync(attachment, key);
                model.AttachmentUrl = url;
                _db.Tickets.Update(model);
                await _db.SaveChangesAsync();
            }

            // send SQS message
            if (!string.IsNullOrEmpty(model.Subject))
            {
                await _sqsService.SendTicketCreatedAsync(model);
            }

            TempData["Success"] = $"Ticket #{model.Id} created";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        // POST: /Tickets/ChangeStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, TicketStatus status)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();
            ticket.Status = status;
            _db.Tickets.Update(ticket);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Ticket #{id} status updated to {status}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
