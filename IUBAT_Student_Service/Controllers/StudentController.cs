using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IUBAT_Student_Service.Data;
using IUBAT_Student_Service.Models;
using IUBAT_Student_Service.Models.ViewModels;

namespace IUBAT_Student_Service.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> MyRequests()
        {
            var studentId = _userManager.GetUserId(User);
            var requests = await _context.ServiceRequests
                .Where(r => r.StudentId == studentId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            var viewModels = requests.Select(r => new ServiceRequestDetailViewModel
            {
                Id = r.Id,
                RequestType = r.RequestType,
                Description = r.Description,
                Status = r.Status,
                CreatedDate = r.CreatedDate,
                UpdatedDate = r.UpdatedDate,
                StudentName = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "",
                StudentEmail = currentUser?.Email ?? "",
                StudentIdNumber = currentUser?.StudentId ?? "—"
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ServiceRequestViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var studentId = _userManager.GetUserId(User);
                var serviceRequest = new ServiceRequest
                {
                    StudentId = studentId!,
                    RequestType = model.RequestType,
                    Description = model.Description,
                    Status = RequestStatus.Pending,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.ServiceRequests.Add(serviceRequest);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Request submitted successfully!";
                return RedirectToAction(nameof(MyRequests));
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var studentId = _userManager.GetUserId(User);
            var request = await _context.ServiceRequests
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == id && r.StudentId == studentId);

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
        public async Task<IActionResult> Delete(int id)
        {
            var studentId = _userManager.GetUserId(User);
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.Id == id && r.StudentId == studentId);

            if (request == null) return NotFound();

            // Business rule: students can delete only their own pending requests.
            // If you want to allow deleting any status, remove this check.
            if (request.Status != RequestStatus.Pending)
            {
                TempData["Error"] = $"Request #{request.Id} cannot be deleted because it is {request.Status}. Only Pending requests can be deleted.";
                return RedirectToAction(nameof(MyRequests));
            }

            _context.ServiceRequests.Remove(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Request #{id} deleted successfully.";
            return RedirectToAction(nameof(MyRequests));
        }

        // Alternative: allow deleting any owned request regardless of status (uncomment to use)
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> DeleteAny(int id) { ... }

        [HttpGet]
        public async Task<IActionResult> DeleteConfirm(int? id)
        {
            if (id == null) return NotFound();
            var studentId = _userManager.GetUserId(User);
            var request = await _context.ServiceRequests
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == id && r.StudentId == studentId);
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
    }
}
