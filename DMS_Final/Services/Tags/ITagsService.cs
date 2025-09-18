using DMS_Final.Models;
using System.Collections.Generic;

namespace DMS_Final.Services.Document
{
    public interface ITagsService
    {
        List<TagModel> GetAllTags();
        int AddTagIfNotExists(string tagName);
        void AddDocumentTags(int documentId, List<string> tags);
    }
}
