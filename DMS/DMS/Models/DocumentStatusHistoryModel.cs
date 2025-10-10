namespace DMS.Models
{
    public class DocumentStatusHistoryModel
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int DocumentDetailId { get; set; }
        public string ApproveStatus { get; set; } // Pending, Approved, Rejected
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedFrom { get; set; }
        public string Notes { get; set; }
    }
}
