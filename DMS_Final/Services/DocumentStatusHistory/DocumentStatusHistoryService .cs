using DMS_Final.Models;
using DMS_Final.Repositories;
using System.Collections.Generic;

namespace DMS_Final.Services
{
    public class DocumentStatusHistoryService : IDocumentStatusHistoryService
    {
        private readonly IDocumentStatusHistoryRepository _documentStatusHistoryRepository;

        public DocumentStatusHistoryService(IDocumentStatusHistoryRepository documentStatusHistoryRepository)
        {
            _documentStatusHistoryRepository = documentStatusHistoryRepository;
        }

        public IEnumerable<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)> GetAll()
        {
            return _documentStatusHistoryRepository.GetAll();
        }

        public IEnumerable<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)> GetRecentActivities(int count = 100)
        {
            return _documentStatusHistoryRepository.GetRecentActivities(count);
        }
    }
}