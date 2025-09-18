using DMS_Final.Models;
using System.Data.SqlClient;

namespace DMS_Final.Repository.Pagination
{
    public interface IGenericPaginationRepository
    {
        PagedResultModel<T> GetPaged<T>(
            string countQuery,
            string sqlQuery,
            int pageNumber,
            int pageSize,
            Func<SqlDataReader, T> mapFunction);
    }
}
