using DMS_Final.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DMS_Final.Repositories
{
    public class DocumentStatusHistoryRepository : IDocumentStatusHistoryRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DocumentStatusHistoryRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IEnumerable<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)> GetAll()
        {
            var historyList = new List<(DocumentStatusHistoryModel, string, string)>();
            string sqlQuery = @"
        SELECT dsh.*, d.Title, dd.OriginalFileName
        FROM DocumentStatusHistories dsh
        LEFT JOIN Documents d ON dsh.DocumentId = d.Id
        LEFT JOIN DocumentDetails dd ON dsh.DocumentDetailId = dd.Id
ORDER BY CreatedOn DESC
    ";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
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
                        historyList.Add((history, title, originalFileName));
                    }
                }
            }
            return historyList;
        }

        // Recent Activities
        public IEnumerable<(DocumentStatusHistoryModel History, string Title, string OriginalFileName)> GetRecentActivities(int count = 100)
        {
            var historyList = new List<(DocumentStatusHistoryModel, string, string)>();
            string sqlQuery = @"
        SELECT TOP (@Count) dsh.*, d.Title, dd.OriginalFileName
        FROM DocumentStatusHistories dsh
        LEFT JOIN Documents d ON dsh.DocumentId = d.Id
        LEFT JOIN DocumentDetails dd ON dsh.DocumentDetailId = dd.Id
        ORDER BY dsh.CreatedOn DESC";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
            {
                command.Parameters.AddWithValue("@Count", count);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
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
                        historyList.Add((history, title, originalFileName));
                    }
                }
            }
            return historyList;
        }

        // Recent Activities for notification, including Document Title and OriginalFileName
        //public IEnumerable<(DocumentStatusHistoryModel History, string DocumentTitle, string OriginalFileName)> GetRecentNotifications(int count = 10)
        //{
        //    var notificationList = new List<(DocumentStatusHistoryModel, string, string)>();
        //    string sqlQuery = @"
        //        SELECT TOP (@Count) h.Id, h.DocumentId, h.DocumentDetailId, h.ApproveStatus, h.CreatedBy, h.CreatedOn, h.CreatedFrom, h.Notes,
        //               d.Title AS DocumentTitle, dd.OriginalFileName
        //        FROM DocumentStatusHistories h
        //        INNER JOIN Documents d ON h.DocumentId = d.Id
        //        INNER JOIN DocumentDetails dd ON h.DocumentDetailId = dd.Id
        //        ORDER BY h.CreatedOn DESC";

        //    using (SqlConnection connection = new SqlConnection(_connectionString))
        //    {
        //        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
        //        {
        //            command.Parameters.AddWithValue("@Count", count);
        //            connection.Open();
        //            using (SqlDataReader reader = command.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    var history = new DocumentStatusHistoryModel
        //                    {
        //                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                        DocumentId = reader.GetInt32(reader.GetOrdinal("DocumentId")),
        //                        DocumentDetailId = reader.GetInt32(reader.GetOrdinal("DocumentDetailId")),
        //                        ApproveStatus = reader.GetString(reader.GetOrdinal("ApproveStatus")),
        //                        CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")),
        //                        CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
        //                        CreatedFrom = reader.GetString(reader.GetOrdinal("CreatedFrom")),
        //                        Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? string.Empty : reader.GetString(reader.GetOrdinal("Notes"))
        //                    };

        //                    string documentTitle = reader.GetString(reader.GetOrdinal("DocumentTitle"));
        //                    string originalFileName = reader.GetString(reader.GetOrdinal("OriginalFileName"));

        //                    notificationList.Add((history, documentTitle, originalFileName));
        //                }
        //            }
        //        }
        //    }
        //    return notificationList;
        //}
    }
}
