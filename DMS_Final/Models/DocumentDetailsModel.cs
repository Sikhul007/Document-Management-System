namespace DMS_Final.Models
{
    public class DocumentDetailsModel
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string OriginalFileName { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public int VersionNumber { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedFrom { get; set; }

        public string? LastUpdateBy { get; set; }
        public DateTime? LastUpdateOn { get; set; }
        public string? LastUpdateFrom { get; set; }

        public string ApproveStatus { get; set; }
        public bool IsArchive { get; set; }
        public int? ParentDocumentId { get; set; }




        public DateTime FileUploadedTime { get; set; }
        public string Title { get; set; }
        public DateTime UploadedDate { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public string Notes { get; set; }
    }
}
