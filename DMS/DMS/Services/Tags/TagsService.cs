using DMS.Models;
using DMS.Repository.Document;
using DMS.Repository.DocumentTags;
using DMS.Repository.Tags;

namespace DMS.Services.Document
{
    public class TagsService : ITagsService
    {
        private readonly ITagsRepository _tagsRepo;
        private readonly IDocumentTagsRepository _docTagsRepo;

        public TagsService(ITagsRepository tagsRepo, IDocumentTagsRepository docTagsRepo)
        {
            _tagsRepo = tagsRepo;
            _docTagsRepo = docTagsRepo;
        }

        public List<TagModel> GetAllTags() => _tagsRepo.GetAllTags();

        public int AddTagIfNotExists(string tagName)
        {
            var existing = _tagsRepo.GetAllTags().FirstOrDefault(t => t.Name.ToLower() == tagName.ToLower());
            if (existing != null) return existing.Id;
            return _tagsRepo.AddTag(tagName);
        }

        public void AddDocumentTags(int documentId, List<string> tags)
        {
            if (tags == null || tags.Count == 0) return;

            foreach (var tagName in tags)
            {
               
                int tagId = AddTagIfNotExists(tagName);

                
                _docTagsRepo.AddDocumentTag(documentId, tagId);
            }
        }


    }
}
