using DMS.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace DMS.Repository.Pagination
{
    public class GenericPaginationRepository : IGenericPaginationRepository
    {
        private readonly string _connectionString;

        public GenericPaginationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public PagedResultModel<T> GetPaged<T>(
            string countQuery,
            string sqlQuery,
            int pageNumber,
            int pageSize,
            Func<SqlDataReader, T> mapFunction)
        {
            var items = new List<T>();
            int totalRecords = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Count total records
                using (SqlCommand countCmd = new SqlCommand(countQuery, connection))
                {
                    totalRecords = (int)countCmd.ExecuteScalar();
                }

                // Apply pagination query
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    int offset = (pageNumber - 1) * pageSize;
                    command.Parameters.AddWithValue("@Offset", offset);
                    command.Parameters.AddWithValue("@PageSize", pageSize);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(mapFunction(reader));
                        }
                    }
                }
            }

            return new PagedResultModel<T>
            {
                Items = items,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
