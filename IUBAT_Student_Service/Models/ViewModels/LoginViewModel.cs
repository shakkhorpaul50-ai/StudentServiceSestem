using System.ComponentModel.DataAnnotations;

namespace IUBAT_Student_Service.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Student ID")]
        [StringLength(50)]
        // Required for Students, optional for Staff — validated in controller.
        // Keep UI hint; server enforces uniqueness/match for Student role.
        public string? StudentId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
