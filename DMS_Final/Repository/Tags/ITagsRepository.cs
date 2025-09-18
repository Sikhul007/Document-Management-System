using DMS_Final.Models;

namespace DMS_Final.Repository.Tags
{
    public interface ITagsRepository
    {
        List<TagModel> GetAllTags();
        int AddTag(string tagName);
        bool TagExists(string tagName);
        void AddDocumentTags(int documentId, List<string> tagNames);
        List<string> GetDocumentTags(int documentId);
        void RemoveDocumentTags(int documentId);
    }
}