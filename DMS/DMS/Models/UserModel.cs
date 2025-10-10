using System.ComponentModel.DataAnnotations;

namespace DMS.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string UserName { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(100), EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(500)]
        public string Roles { get; set; }

        [Required, StringLength(256)]
        public string Password { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required, StringLength(20)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [StringLength(20)]
        public string? LastUpdateBy { get; set; }

        public DateTime? LastUpdateOn { get; set; }

        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastLogoutTime { get; set; }

    }
}
