using DMS_Final.Models;
using DMS_Final.Repository.Pagination;
using DMS_Final.Services.Pagination;

public class DocumentStatusHistoryPaginationService : IDocumentStatusHistoryPaginationService
{
    private readonly IGenericPaginationRepository _paginationRepository;

    public DocumentStatusHistoryPaginationService(IGenericPaginationRepository paginationRepository)
    {
        _paginationRepository = paginationRepository;
    }

    public PagedResultModel<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)>
        GetPaged(int pageNumber, int pageSize)
    {
        string countQuery = "SELECT COUNT(*) FROM DocumentStatusHistories";
        string sqlQuery = @"
                SELECT dsh.*, d.Title, dd.OriginalFileName
                FROM DocumentStatusHistories dsh
                LEFT JOIN Documents d ON dsh.DocumentId = d.Id
                LEFT JOIN DocumentDetails dd ON dsh.DocumentDetailId = dd.Id
                ORDER BY dsh.CreatedOn DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

        return _paginationRepository.GetPaged(
            countQuery,
            sqlQuery,
            pageNumber,
            pageSize,
            reader =>
            {
                var history = new DocumentStatusHistoryModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    DocumentId = reader.GetInt32(reader.GetOrdinal("DocumentId")),
                    DocumentDetailId = reader.GetInt32(reader.GetOrdinal("DocumentDetailId")),
                    ApproveStatus = reader.GetString(reader.GetOrdinal("ApproveStatus")),
                    CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")),
                    CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
                    CreatedFrom = reader.GetString(reader.GetOrdinal("CreatedFrom")),
                    Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? string.Empty : reader.GetString(reader.GetOrdinal("Notes"))
                };

                string title = reader.IsDBNull(reader.GetOrdinal("Title")) ? string.Empty : reader.GetString(reader.GetOrdinal("Title"));
                string originalFileName = reader.IsDBNull(reader.GetOrdinal("OriginalFileName")) ? string.Empty : reader.GetString(reader.GetOrdinal("OriginalFileName"));

                return (history, title, originalFileName);
            }
        );
    }
}