using DMS_Final.Models;
using DMS_Final.Repository.Pagination;
using System.Data.SqlClient;

namespace DMS_Final.Services.Pagination.DocumentStatusHistory
{
    public interface IDocumentStatusHistoryPaginationService
    {
        PagedResultModel<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)>
            GetPaged(int pageNumber, int pageSize);
    }
}
