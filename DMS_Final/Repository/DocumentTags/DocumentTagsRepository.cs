using DMS_Final.Models;
using DMS_Final.Repository.DocumentTags;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace DMS_Final.Repository.Document
{
    public class DocumentTagsRepository : IDocumentTagsRepository
    {
        private readonly string _connectionString;

        public DocumentTagsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void AddDocumentTag(int documentId, int tagId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var query = "INSERT INTO DocumentTags (DocumentId, TagId) VALUES (@DocumentId, @TagId)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);
                    cmd.Parameters.AddWithValue("@TagId", tagId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}