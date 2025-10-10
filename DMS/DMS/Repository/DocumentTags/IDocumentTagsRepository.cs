namespace DMS.Repository.DocumentTags
{
    public interface IDocumentTagsRepository
    {
        void AddDocumentTag(int documentId, int tagId);
    }
}

