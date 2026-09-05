using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IUBAT_Student_Service.Data;
using IUBAT_Student_Service.Models;
using IUBAT_Student_Service.Models.ViewModels;

namespace IUBAT_Student_Service.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AllRequests(string? searchId, string? searchTerm)
        {
            // Support both param names: searchId (spec) and searchTerm (alias)
            var query = (searchId ?? searchTerm)?.Trim();
            ViewData["CurrentFilter"] = query;

            var dbQuery = _context.ServiceRequests
                .Include(r => r.Student)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowered = query.ToLower();
                // Search by: Request ID exact, StudentId (unique), Student Email, Student Name
                // Also handle numeric request id
                bool isNumeric = int.TryParse(query, out var reqId);
                dbQuery = dbQuery.Where(r =>
                    (isNumeric && r.Id == reqId) ||
                    (r.Student != null && r.Student.StudentId != null && r.Student.StudentId.ToLower().Contains(lowered)) ||
                    (r.Student != null && r.Student.Email != null && r.Student.Email.ToLower().Contains(lowered)) ||
                    (r.Student != null && (r.Student.FirstName + " " + r.Student.LastName).ToLower().Contains(lowered)) ||
                    r.Description.ToLower().Contains(lowered) ||
                    r.RequestType.ToString().ToLower().Contains(lowered)
                );
            }

            var requests = await dbQuery
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            var viewModels = requests.Select(r => new ServiceRequestDetailViewModel
            {
                Id = r.Id,
                RequestType = r.RequestType,
                Description = r.Description,
                Status = r.Status,
                CreatedDate = r.CreatedDate,
                UpdatedDate = r.UpdatedDate,
                StudentName = r.Student?.FirstName + " " + r.Student?.LastName,
                StudentEmail = r.Student?.Email ?? "",
                StudentIdNumber = r.Student?.StudentId ?? "—"
            }).ToList();

            return View(viewModels);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var request = await _context.ServiceRequests
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            var viewModel = new ServiceRequestDetailViewModel
            {
                Id = request.Id,
                RequestType = request.RequestType,
                Description = request.Description,
                Status = request.Status,
                CreatedDate = request.CreatedDate,
                UpdatedDate = request.UpdatedDate,
                StudentName = request.Student?.FirstName + " " + request.Student?.LastName,
                StudentEmail = request.Student?.Email ?? "",
                StudentIdNumber = request.Student?.StudentId ?? "—"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, StaffUpdateViewModel model)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = model.Status;
            request.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Status updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
