namespace DMS_Final.Models
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public int TargetUserId { get; set; }
        public int ActorUserId { get; set; }
        public string Message { get; set; }
        public int? DocumentId { get; set; }
        public int? DocumentDetailId { get; set; }
        public string EventType { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}