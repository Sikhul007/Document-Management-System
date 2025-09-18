using DMS_Final.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DMS_Final.Repository.Tags
{
    public class TagsRepository : ITagsRepository
    {
        private readonly string _connectionString;

        public TagsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<TagModel> GetAllTags()
        {
            var tags = new List<TagModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT Id, Name FROM Tags ORDER BY Name";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tags.Add(new TagModel
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name")
                            });
                        }
                    }
                }
            }
            return tags;
        }

        public int AddTag(string tagName)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Tags (Name) VALUES (@Name);
                              SELECT CAST(SCOPE_IDENTITY() AS int);";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", tagName.Trim());
                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public bool TagExists(string tagName)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM Tags WHERE Name = @Name";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", tagName.Trim());
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public void AddDocumentTags(int documentId, List<string> tagNames)
        {
            if (tagNames == null || tagNames.Count == 0) return;
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                foreach (string tagName in tagNames)
                {
                    if (string.IsNullOrWhiteSpace(tagName)) continue;
                    string trimmedTag = tagName.Trim();
                    // Add tag to Tags table if it doesn't exist
                    //if (!TagExists(trimmedTag))
                    //{
                    //    AddTag(trimmedTag);
                    //}
                    // Add to DocumentTags table
                    string sql = @"INSERT INTO DocumentTags (DocumentId, TagName)
                                  VALUES (@DocumentId, @TagName)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@DocumentId", documentId);
                        cmd.Parameters.AddWithValue("@TagName", trimmedTag);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<string> GetDocumentTags(int documentId)
        {
            var tags = new List<string>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT TagName FROM DocumentTags WHERE DocumentId = @DocumentId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tags.Add(reader.GetString("TagName"));
                        }
                    }
                }
            }
            return tags;
        }

        public void RemoveDocumentTags(int documentId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "DELETE FROM DocumentTags WHERE DocumentId = @DocumentId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}