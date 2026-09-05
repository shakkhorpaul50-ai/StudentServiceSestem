using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IUBAT_Student_Service.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Student ID provided by the student at registration/login.
        /// Must be unique among students. Nullable for Staff accounts.
        /// </summary>
        [StringLength(50)]
        public string? StudentId { get; set; }
    }
}
