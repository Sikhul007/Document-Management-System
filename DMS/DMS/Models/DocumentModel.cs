using System.ComponentModel.DataAnnotations;

namespace DMS.Models
{
    public class DocumentModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        public DateTime UploadedDate { get; set; }
        public string Description { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedFrom { get; set; }

        public string? LastUpdateBy { get; set; }
        public DateTime? LastUpdateOn { get; set; }
        public string LastUpdateFrom { get; set; }
    }
}
