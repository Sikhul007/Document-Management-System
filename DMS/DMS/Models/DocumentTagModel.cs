namespace DMS.Models
{
    public class DocumentTagModel
    {
        public int Id { get; set; }
        public int DocumentDetailsId { get; set; }
        public string TagName { get; set; }
    }
}
