using DMS.Models;
using DMS.Repository.Pagination;
using System.Data.SqlClient;

namespace DMS.Services.Pagination.DocumentStatusHistory
{
    public interface IDocumentStatusHistoryPaginationService
    {
        PagedResultModel<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)>
            GetPaged(int pageNumber, int pageSize);
    }
}
