using DMS.Models;
using System.Data.SqlClient;

namespace DMS.Repository.Pagination
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
