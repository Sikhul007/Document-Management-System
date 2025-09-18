using DMS_Final.Models;
using System.Collections.Generic;

namespace DMS_Final.Repositories
{
    public interface IDocumentStatusHistoryRepository
    {
        IEnumerable<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)> GetAll();
        IEnumerable<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)> GetRecentActivities(int count = 100);
        //IEnumerable<(DocumentStatusHistoryModel History, string DocumentTitle, string OriginalFileName)> GetRecentNotifications(int count = 10);
    }
}