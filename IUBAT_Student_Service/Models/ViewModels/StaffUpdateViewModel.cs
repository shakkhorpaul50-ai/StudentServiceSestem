using System.ComponentModel.DataAnnotations;

namespace IUBAT_Student_Service.Models.ViewModels
{
    public class StaffUpdateViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Status")]
        public RequestStatus Status { get; set; }
    }
}
