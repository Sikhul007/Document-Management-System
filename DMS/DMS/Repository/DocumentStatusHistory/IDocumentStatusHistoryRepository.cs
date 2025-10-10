using DMS.Models;
using System.Collections.Generic;

namespace DMS.Repositories
{
    public interface IDocumentStatusHistoryRepository
    {
        IEnumerable<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)> GetAll();
        IEnumerable<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)> GetRecentActivities(int count = 100);
        //IEnumerable<(DocumentStatusHistoryModel History, string DocumentTitle, string OriginalFileName)> GetRecentNotifications(int count = 10);
    }
}