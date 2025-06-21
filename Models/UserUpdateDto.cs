using System.ComponentModel.DataAnnotations;

namespace CrudApiDemo.Models.Dto
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active";
    }
}
