using DMS_Final.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection.Metadata;

namespace DMS_Final.Repository.Document
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly string _connectionString;

        public DocumentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public int InsertDocument(DocumentModel document, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"INSERT INTO Documents (Title, Description, CreatedBy, CreatedOn, CreatedFrom)
                           VALUES (@Title, @Description, @CreatedBy, @CreatedOn, @CreatedFrom);
                           SELECT SCOPE_IDENTITY();";
            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Title", document.Title);
                cmd.Parameters.AddWithValue("@Description", (object)document.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", document.CreatedBy);
                cmd.Parameters.AddWithValue("@CreatedOn", document.CreatedOn);
                cmd.Parameters.AddWithValue("@CreatedFrom", (object)document.CreatedFrom ?? DBNull.Value);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        public List<DocumentModel> GetDocumentsByUser(string userName)
        {
            var list = new List<DocumentModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT * FROM Documents WHERE CreatedBy = @CreatedBy ORDER BY CreatedOn DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CreatedBy", userName);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DocumentModel
                            {
                                Id = (int)reader["Id"],
                                Title = reader["Title"].ToString(),
                                UploadedDate = (DateTime)reader["UploadedDate"],
                                Description = reader["Description"]?.ToString(),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                CreatedFrom = reader["CreatedFrom"]?.ToString(),
                                LastUpdateBy = reader["LastUpdateBy"]?.ToString(),
                                LastUpdateOn = reader["LastUpdateOn"] as DateTime?,
                                LastUpdateFrom = reader["LastUpdateFrom"]?.ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }


        public int InsertDocumentDetails(DocumentDetailsModel details, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"INSERT INTO DocumentDetails
                           (DocumentId, OriginalFileName, FileName, Description, VersionNumber, CreatedBy, CreatedOn, ApproveStatus, IsArchive, ParentDocumentId, LastUpdateBy, LastUpdateOn)
                           VALUES
                           (@DocumentId, @OriginalFileName, @FileName, @Description, @VersionNumber, @CreatedBy, @CreatedOn, @ApproveStatus, @IsArchive, @ParentDocumentId, @LastUpdateBy, @LastUpdateOn);
                           SELECT SCOPE_IDENTITY();";
            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DocumentId", details.DocumentId);
                cmd.Parameters.AddWithValue("@OriginalFileName", details.OriginalFileName);
                cmd.Parameters.AddWithValue("@FileName", details.FileName);
                cmd.Parameters.AddWithValue("@Description", details.Description);
                cmd.Parameters.AddWithValue("@VersionNumber", details.VersionNumber);
                cmd.Parameters.AddWithValue("@CreatedBy", details.CreatedBy);
                cmd.Parameters.AddWithValue("@CreatedOn", details.CreatedOn);
                cmd.Parameters.AddWithValue("@ApproveStatus", details.ApproveStatus);
                cmd.Parameters.AddWithValue("@IsArchive", details.IsArchive);
                cmd.Parameters.AddWithValue("@ParentDocumentId", details.ParentDocumentId ?? 0);
                cmd.Parameters.AddWithValue("@LastUpdateBy", (object)details.LastUpdateBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LastUpdateOn", (object)details.LastUpdateOn ?? DBNull.Value);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }


        public void SetArchiveStatus(int documentDetailId, bool isArchive, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = "UPDATE DocumentDetails SET IsArchive = @IsArchive WHERE Id = @Id";
            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@IsArchive", isArchive);
                cmd.Parameters.AddWithValue("@Id", documentDetailId);
                cmd.ExecuteNonQuery();
            }
        }


        public void SetApproveStatus(int documentDetailId, string status, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = "UPDATE DocumentDetails SET ApproveStatus = @Status WHERE Id = @Id";
            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Id", documentDetailId);
                cmd.ExecuteNonQuery();
            }
        }


        public void SetLastUpdateInfo(int documentDetailId, string lastUpdateBy, DateTime lastUpdateOn, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = "UPDATE DocumentDetails SET LastUpdateBy = @LastUpdateBy, LastUpdateOn = @LastUpdateOn WHERE Id = @Id";
            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@LastUpdateBy", lastUpdateBy);
                cmd.Parameters.AddWithValue("@LastUpdateOn", lastUpdateOn);
                cmd.Parameters.AddWithValue("@Id", documentDetailId);
                cmd.ExecuteNonQuery();
            }
        }


        public List<DocumentModel> GetMyDocumentDetails(string userName)
        {
            var list = new List<DocumentModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                                SELECT Id, Title, Description
                                FROM Documents
                                WHERE CreatedBy = @CreatedBy ORDER BY CreatedOn DESC
                                ";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CreatedBy", userName);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var model = new DocumentModel
                            {
                                Id = (int)reader["Id"],
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"]?.ToString()
                            };
                            list.Add(model);
                        }
                    }
                }
            }
            return list;
        }


        // FIXED GetLatestFilesByDocumentId method
        public List<DocumentDetailsModel> GetLatestFilesByDocumentId(int documentId)
        {
            var list = new List<DocumentDetailsModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                // Simplified query to get latest non-archived files
                string sql = @"
                SELECT dd.*
                FROM DocumentDetails dd
                WHERE dd.DocumentId = @DocumentId 
                
                AND dd.ApproveStatus IN ('pending', 'approved')
                ORDER BY dd.Id";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detail = new DocumentDetailsModel
                            {
                                Id = (int)reader["Id"],
                                DocumentId = (int)reader["DocumentId"],
                                OriginalFileName = reader["OriginalFileName"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Description = reader["Description"].ToString(),
                                VersionNumber = (int)reader["VersionNumber"],
                                ApproveStatus = reader["ApproveStatus"].ToString(),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                IsArchive = Convert.ToInt32(reader["IsArchive"]) == 1,
                                ParentDocumentId = reader["ParentDocumentId"] == DBNull.Value ? (int?)null : (int)reader["ParentDocumentId"],
                                Tags = new List<string>()
                            };
                            list.Add(detail);
                        }
                    }

                    // Load tags for each detail
                    foreach (var detail in list)
                    {
                        string sqlTags = "SELECT TagName FROM DocumentTags WHERE DocumentDetailsId = @DocumentDetailsId";
                        using (var cmdTags = new SqlCommand(sqlTags, conn))
                        {
                            cmdTags.Parameters.AddWithValue("@DocumentDetailsId", detail.Id);
                            using (var readerTags = cmdTags.ExecuteReader())
                            {
                                while (readerTags.Read())
                                {
                                    detail.Tags.Add(readerTags["TagName"].ToString());
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }


        public List<DocumentDetailsModel> GetPendingDocumentDetailsWithHeader(string userName = null)
        {
            var list = new List<DocumentDetailsModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT d.Id AS DocumentId, d.Title,
                        dd.CreatedOn AS FileUploadedTime,
                        d.UploadedDate, d.Description AS DocumentDescription,
                        d.CreatedBy AS DocumentCreatedBy,
                        d.CreatedOn AS DocumentCreatedOn,
                        dd.Id, dd.OriginalFileName, dd.VersionNumber, dd.ApproveStatus, dd.Description
                        FROM DocumentDetails dd
                        INNER JOIN Documents d ON dd.DocumentId = d.Id
                        WHERE dd.ApproveStatus = 'pending'";

                // Correctly append the AND clause after the WHERE clause
                if (!string.IsNullOrEmpty(userName))
                {
                    sql += " AND d.CreatedBy = @UserName";
                }

                // The ORDER BY clause should be at the very end
                sql += " ORDER BY DocumentCreatedOn DESC;";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(userName))
                        cmd.Parameters.AddWithValue("@UserName", userName);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var model = new DocumentDetailsModel
                            {
                                Id = (int)reader["Id"],
                                DocumentId = (int)reader["DocumentId"],
                                OriginalFileName = reader["OriginalFileName"].ToString(),
                                VersionNumber = (int)reader["VersionNumber"],
                                ApproveStatus = reader["ApproveStatus"].ToString(),
                                Title = reader["Title"].ToString(),
                                UploadedDate = (DateTime)reader["UploadedDate"],
                                Description = reader["Description"].ToString(),
                                CreatedBy = reader["DocumentCreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["DocumentCreatedOn"],
                                FileUploadedTime = (DateTime)reader["FileUploadedTime"]
                            };
                            list.Add(model);
                        }
                    }
                }
            }
            return list;
        }


        // DocumentStatusHistories
        public void InsertStatusHistory(DocumentStatusHistoryModel history, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"INSERT INTO DocumentStatusHistories
                           (DocumentId, DocumentDetailId, ApproveStatus, CreatedBy, CreatedOn, CreatedFrom, Notes)
                           VALUES
                           (@DocumentId, @DocumentDetailId, @ApproveStatus, @CreatedBy, @CreatedOn, @CreatedFrom, @Notes)";
            using (SqlCommand cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DocumentId", history.DocumentId);
                cmd.Parameters.AddWithValue("@DocumentDetailId", history.DocumentDetailId);
                cmd.Parameters.AddWithValue("@ApproveStatus", history.ApproveStatus);
                cmd.Parameters.AddWithValue("@CreatedBy", history.CreatedBy);
                cmd.Parameters.AddWithValue("@CreatedOn", history.CreatedOn);
                cmd.Parameters.AddWithValue("@CreatedFrom", (object)history.CreatedFrom ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Notes", (object)history.Notes ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }


        public DocumentModel GetDocumentById(int documentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT Id, Title, Description, CreatedBy, CreatedOn, CreatedFrom, LastUpdateBy, LastUpdateOn, LastUpdateFrom
                       FROM Documents WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", documentId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new DocumentModel
                            {
                                Id = (int)reader["Id"],
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"]?.ToString(),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                CreatedFrom = reader["CreatedFrom"]?.ToString(),
                                LastUpdateBy = reader["LastUpdateBy"]?.ToString(),
                                LastUpdateOn = reader["LastUpdateOn"] as DateTime?,
                                LastUpdateFrom = reader["LastUpdateFrom"]?.ToString()
                            };
                        }
                    }
                }
            }
            return null;
        }


        // FIXED GetDocumentDetailById method - CRITICAL FIX
        public DocumentDetailsModel GetDocumentDetailById(int documentDetailId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT Id,DocumentId,OriginalFileName,FileName,Description,VersionNumber,CreatedBy,CreatedOn,CreatedFrom,LastUpdateBy,LastUpdateOn,LastUpdateFrom,ApproveStatus, IsArchive,ParentDocumentId
                               FROM DocumentDetails
                               WHERE Id = @Id";

                DocumentDetailsModel detail = null;
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", documentDetailId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            detail = new DocumentDetailsModel
                            {
                                Id = (int)reader["Id"],
                                DocumentId = (int)reader["DocumentId"],
                                OriginalFileName = reader["OriginalFileName"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Description = reader["Description"].ToString(),
                                VersionNumber = (int)reader["VersionNumber"],
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                CreatedFrom = reader["CreatedFrom"] == DBNull.Value ? null : reader["CreatedFrom"].ToString(),
                                LastUpdateBy = reader["LastUpdateBy"] == DBNull.Value ? null : reader["LastUpdateBy"].ToString(),
                                LastUpdateOn = reader["LastUpdateOn"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["LastUpdateOn"],
                                LastUpdateFrom = reader["LastUpdateFrom"] == DBNull.Value ? null : reader["LastUpdateFrom"].ToString(),
                                ApproveStatus = reader["ApproveStatus"].ToString(),
                                IsArchive = Convert.ToInt32(reader["IsArchive"]) == 1,
                                ParentDocumentId = reader["ParentDocumentId"] == DBNull.Value ? (int?)null : (int)reader["ParentDocumentId"],
                                Tags = new List<string>() // Initialize tags list
                            };
                        }
                    }
                }

                // CRITICAL: Load tags for this document detail
                if (detail != null)
                {
                    string sqlTags = "SELECT TagName FROM DocumentTags WHERE DocumentDetailsId = @DocumentDetailsId";
                    using (var cmdTags = new SqlCommand(sqlTags, conn))
                    {
                        cmdTags.Parameters.AddWithValue("@DocumentDetailsId", detail.Id);
                        using (var readerTags = cmdTags.ExecuteReader())
                        {
                            while (readerTags.Read())
                            {
                                detail.Tags.Add(readerTags["TagName"].ToString());
                            }
                        }
                    }
                }

                return detail;
            }
        }


        public List<TagModel> GetAllTags()
        {
            var tags = new List<TagModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT Id, Name FROM Tags";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tags.Add(new TagModel
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return tags;
        }


        public void InsertDocumentTag(int documentDetailId, int tagId, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"INSERT INTO DocumentTags (DocumentDetailsId, TagName)
                   VALUES (@DocumentDetailsId, (SELECT Name FROM Tags WHERE Id = @TagId))";
            using (var cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@DocumentDetailsId", documentDetailId);
                cmd.Parameters.AddWithValue("@TagId", tagId);
                cmd.ExecuteNonQuery();
            }
        }


        public List<DocumentTagModel> GetTagsByDocumentDetailsId(int documentDetailsId)
        {
            var tags = new List<DocumentTagModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT Id, DocumentDetailsId, TagName
                       FROM DocumentTags
                       WHERE DocumentDetailsId = @DocumentDetailsId";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentDetailsId", documentDetailsId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tags.Add(new DocumentTagModel
                            {
                                Id = (int)reader["Id"],
                                DocumentDetailsId = (int)reader["DocumentDetailsId"],
                                TagName = reader["TagName"].ToString()
                            });
                        }
                    }
                }
            }
            return tags;
        }


        public List<DocumentModel> GetAllDocuments()
        {
            var list = new List<DocumentModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Documents ORDER BY CreatedOn DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DocumentModel
                            {
                                Id = (int)reader["Id"],
                                Title = reader["Title"].ToString(),
                                UploadedDate = (DateTime)reader["UploadedDate"],
                                Description = reader["Description"]?.ToString(),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                CreatedFrom = reader["CreatedFrom"]?.ToString(),
                                LastUpdateBy = reader["LastUpdateBy"]?.ToString(),
                                LastUpdateOn = reader["LastUpdateOn"] as DateTime?,
                                LastUpdateFrom = reader["LastUpdateFrom"]?.ToString()
                            });
                        }
                    }
                }
            }
            return list;
        }


        public List<DocumentDetailsModel> GetDocumentDetailsByDocumentId(int documentId)
        {
            var details = new List<DocumentDetailsModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sqlDetails = "SELECT * FROM DocumentDetails WHERE DocumentId = @DocumentId AND ApproveStatus IN ('approved', 'pending') ORDER BY CreatedOn DESC";
                using (var cmdDetails = new SqlCommand(sqlDetails, conn))
                {
                    cmdDetails.Parameters.AddWithValue("@DocumentId", documentId);
                    conn.Open();
                    using (var reader = cmdDetails.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detail = new DocumentDetailsModel
                            {
                                Id = (int)reader["Id"],
                                DocumentId = (int)reader["DocumentId"],
                                OriginalFileName = reader["OriginalFileName"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Description = reader["Description"].ToString(),
                                VersionNumber = (int)reader["VersionNumber"],
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                ApproveStatus = reader["ApproveStatus"].ToString(),
                                IsArchive = Convert.ToInt32(reader["IsArchive"]) == 1,
                                ParentDocumentId = reader["ParentDocumentId"] == DBNull.Value ? (int?)null : (int)reader["ParentDocumentId"],
                                Tags = new List<string>()
                            };
                            details.Add(detail);
                        }
                    }
                    // For each detail, get tags
                    foreach (var detail in details)
                    {
                        string sqlTags = "SELECT TagName FROM DocumentTags WHERE DocumentDetailsId = @DocumentDetailsId";
                        using (var cmdTags = new SqlCommand(sqlTags, conn))
                        {
                            cmdTags.Parameters.AddWithValue("@DocumentDetailsId", detail.Id);
                            using (var readerTags = cmdTags.ExecuteReader())
                            {
                                while (readerTags.Read())
                                {
                                    detail.Tags.Add(readerTags["TagName"].ToString());
                                }
                            }
                        }
                    }
                }
            }
            return details;
        }


        public List<DocumentDetailsModel> Search(int documentId, string tag, string status, string version)
        {
            var details = new List<DocumentDetailsModel>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT dd.* FROM DocumentDetails dd WHERE dd.DocumentId = @DocumentId AND (dd.ApproveStatus = 'pending' OR dd.ApproveStatus = 'approved')";

                // Tag filter (inside EXISTS to avoid duplicates)
                if (!string.IsNullOrEmpty(tag))
                {
                    sql += @" AND EXISTS (
                          SELECT 1
                          FROM DocumentTags dt
                          WHERE dt.DocumentDetailsId = dd.Id
                            AND dt.TagName LIKE @Tag
                      )";
                }
                else
                {
                    // Only require that at least one tag exists
                    sql += @" AND EXISTS (
                          SELECT 1
                          FROM DocumentTags dt
                          WHERE dt.DocumentDetailsId = dd.Id
                      )";
                }

                if (!string.IsNullOrEmpty(status))
                    sql += " AND dd.ApproveStatus LIKE @Status";

                if (!string.IsNullOrEmpty(version))
                    sql += " AND dd.VersionNumber = @Version";

                sql += " ORDER BY dd.VersionNumber DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);
                    if (!string.IsNullOrEmpty(tag))
                        cmd.Parameters.AddWithValue("@Tag", "%" + tag + "%");
                    if (!string.IsNullOrEmpty(status))
                        cmd.Parameters.AddWithValue("@Status", "%" + status + "%");
                    if (!string.IsNullOrEmpty(version) && int.TryParse(version, out int ver))
                        cmd.Parameters.AddWithValue("@Version", ver);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detail = new DocumentDetailsModel
                            {
                                Id = (int)reader["Id"],
                                DocumentId = (int)reader["DocumentId"],
                                OriginalFileName = reader["OriginalFileName"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Description = reader["Description"].ToString(),
                                VersionNumber = (int)reader["VersionNumber"],
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                ApproveStatus = reader["ApproveStatus"].ToString(),
                                IsArchive = Convert.ToInt32(reader["IsArchive"]) == 1,
                                ParentDocumentId = reader["ParentDocumentId"] == DBNull.Value ? (int?)null : (int)reader["ParentDocumentId"],
                                Tags = new List<string>()
                            };
                            details.Add(detail);
                        }
                    }

                    // Load tags for each document
                    foreach (var detail in details)
                    {
                        string sqlTags = "SELECT TagName FROM DocumentTags WHERE DocumentDetailsId = @DocumentDetailsId";
                        using (var cmdTags = new SqlCommand(sqlTags, conn))
                        {
                            cmdTags.Parameters.AddWithValue("@DocumentDetailsId", detail.Id);
                            using (var readerTags = cmdTags.ExecuteReader())
                            {
                                while (readerTags.Read())
                                {
                                    detail.Tags.Add(readerTags["TagName"].ToString());
                                }
                            }
                        }
                    }
                }
            }
            return details;
        }


        public void DeleteDocumentDetailById(int documentDetailId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // 1. Delete related tags
                string sqlTags = "DELETE FROM DocumentTags WHERE DocumentDetailsId = @Id";
                using (var cmdTags = new SqlCommand(sqlTags, conn))
                {
                    cmdTags.Parameters.AddWithValue("@Id", documentDetailId);
                    cmdTags.ExecuteNonQuery();
                }

                // 2. Delete the document detail
                string sqlDetail = "DELETE FROM DocumentDetails WHERE Id = @Id";
                using (var cmdDetail = new SqlCommand(sqlDetail, conn))
                {
                    cmdDetail.Parameters.AddWithValue("@Id", documentDetailId);
                    cmdDetail.ExecuteNonQuery();
                }
            }
        }


        public (DocumentModel, List<DocumentDetailsModel>) GetDocumentWithDetailsForEdit(int documentId)
        {
            var document = GetDocumentById(documentId);
            var details = GetDocumentDetailsByDocumentId(documentId);
            return (document, details);
        }

        public void UpdateDocumentInfo(int documentId, string title, string description)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Documents SET Title = @Title, Description = @Description WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Id", documentId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public void UpdateDocumentDetailInfo(int documentDetailId, string description, List<int> tagIds)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "UPDATE DocumentDetails SET Description = @Description WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@Id", documentDetailId);
                    cmd.ExecuteNonQuery();
                }
                // Update tags: delete old, insert new
                string deleteTags = "DELETE FROM DocumentTags WHERE DocumentDetailsId = @Id";
                using (var cmdDel = new SqlCommand(deleteTags, conn))
                {
                    cmdDel.Parameters.AddWithValue("@Id", documentDetailId);
                    cmdDel.ExecuteNonQuery();
                }
                foreach (var tagId in tagIds)
                {
                    string insertTag = "INSERT INTO DocumentTags (DocumentDetailsId, TagName) VALUES (@Id, (SELECT Name FROM Tags WHERE Id = @TagId))";
                    using (var cmdTag = new SqlCommand(insertTag, conn))
                    {
                        cmdTag.Parameters.AddWithValue("@Id", documentDetailId);
                        cmdTag.Parameters.AddWithValue("@TagId", tagId);
                        cmdTag.ExecuteNonQuery();
                    }
                }
            }
        }


        // FIXED InsertNewDocumentDetailVersion method
        public void InsertNewDocumentDetailVersion(int documentDetailId, string description, List<int> tagIds, string fileName, string originalFileName, string createdBy, string approveStatus)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Get previous details
                        string getPrevSql = "SELECT Id, DocumentId, VersionNumber FROM DocumentDetails WHERE Id = @Id";
                        DocumentDetailsModel prevDetail = null;
                        using (var cmdPrev = new SqlCommand(getPrevSql, conn, transaction))
                        {
                            cmdPrev.Parameters.AddWithValue("@Id", documentDetailId);
                            using (var reader = cmdPrev.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    prevDetail = new DocumentDetailsModel
                                    {
                                        Id = (int)reader["Id"],
                                        DocumentId = (int)reader["DocumentId"],
                                        VersionNumber = (int)reader["VersionNumber"]
                                    };
                                }
                            }
                        }

                        if (prevDetail == null)
                            throw new Exception("Previous document detail not found.");

                        // Archive previous version
                        string archiveSql = "UPDATE DocumentDetails SET IsArchive = 1, ApproveStatus = 'superseded' WHERE Id = @Id";
                        using (var cmdArchive = new SqlCommand(archiveSql, conn, transaction))
                        {
                            cmdArchive.Parameters.AddWithValue("@Id", documentDetailId);
                            cmdArchive.ExecuteNonQuery();
                        }
                        //string archiveSql = "UPDATE DocumentDetails SET IsArchive = 1, ApproveStatus = 'superseded' WHERE Id = @Id";

                        // Insert new version
                        string insertSql = @"
                        INSERT INTO DocumentDetails
                        (DocumentId, OriginalFileName, FileName, Description, VersionNumber, CreatedBy, CreatedOn, ApproveStatus, IsArchive, ParentDocumentId)
                        VALUES
                        (@DocumentId, @OriginalFileName, @FileName, @Description, @VersionNumber, @CreatedBy, @CreatedOn, @ApproveStatus, @IsArchive, @ParentDocumentId);
                        SELECT SCOPE_IDENTITY();";

                        int newDetailId;
                        using (var cmdInsert = new SqlCommand(insertSql, conn, transaction))
                        {
                            cmdInsert.Parameters.AddWithValue("@DocumentId", prevDetail.DocumentId);
                            cmdInsert.Parameters.AddWithValue("@OriginalFileName", originalFileName ?? string.Empty);
                            cmdInsert.Parameters.AddWithValue("@FileName", fileName ?? string.Empty);
                            cmdInsert.Parameters.AddWithValue("@Description", description ?? string.Empty);
                            cmdInsert.Parameters.AddWithValue("@VersionNumber", prevDetail.VersionNumber + 1);
                            cmdInsert.Parameters.AddWithValue("@CreatedBy", createdBy ?? string.Empty);
                            cmdInsert.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                            cmdInsert.Parameters.AddWithValue("@ApproveStatus", approveStatus);
                            cmdInsert.Parameters.AddWithValue("@IsArchive", 0);
                            cmdInsert.Parameters.AddWithValue("@ParentDocumentId", prevDetail.Id);

                            newDetailId = Convert.ToInt32(cmdInsert.ExecuteScalar());
                        }

                        // Insert tags for new version
                        if (tagIds != null && tagIds.Count > 0)
                        {
                            foreach (var tagId in tagIds)
                            {
                                string insertTag = "INSERT INTO DocumentTags (DocumentDetailsId, TagName) VALUES (@Id, (SELECT Name FROM Tags WHERE Id = @TagId))";
                                using (var cmdTag = new SqlCommand(insertTag, conn, transaction))
                                {
                                    cmdTag.Parameters.AddWithValue("@Id", newDetailId);
                                    cmdTag.Parameters.AddWithValue("@TagId", tagId);
                                    cmdTag.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }



        public List<DocumentDetailsModel> GetDocumentVersionHistoryByUser(string userName, int documentId)
        {
            var result = new List<DocumentDetailsModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
            SELECT d.*, doc.Title
            FROM DocumentDetails d
            INNER JOIN Documents doc ON d.DocumentId = doc.Id
            WHERE d.DocumentId = @DocumentId 
              AND d.ApproveStatus = 'superseded'
              AND d.CreatedBy = @UserName";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@DocumentId", documentId);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var detail = new DocumentDetailsModel
                            {
                                Id = (int)reader["Id"],
                                DocumentId = (int)reader["DocumentId"],
                                OriginalFileName = reader["OriginalFileName"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Description = reader["Description"].ToString(),
                                VersionNumber = (int)reader["VersionNumber"],
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                ApproveStatus = reader["ApproveStatus"].ToString(),
                                IsArchive = reader["IsArchive"] is bool b ? b : Convert.ToInt32(reader["IsArchive"]) == 1,
                                ParentDocumentId = reader["ParentDocumentId"] == DBNull.Value ? (int?)null : (int)reader["ParentDocumentId"],
                                Title = reader["Title"].ToString(), // Title from Documents table
                                Tags = new List<string>()
                            };
                            result.Add(detail);
                        }
                    }
                }

                // Load tags for each detail (reopen connection for each tag query)
                foreach (var detail in result)
                {
                    string tagSql = "SELECT TagName FROM DocumentTags WHERE DocumentDetailsId = @Id";
                    using (var cmdTag = new SqlCommand(tagSql, conn))
                    {
                        cmdTag.Parameters.AddWithValue("@Id", detail.Id);
                        using (var tagReader = cmdTag.ExecuteReader())
                        {
                            while (tagReader.Read())
                            {
                                detail.Tags.Add(tagReader["TagName"].ToString());
                            }
                        }
                    }
                }
            }
            return result;
        }


        public List<DocumentDetailsModel> GetDetailsWithNotes(string createdBy)
        {
            var result = new List<DocumentDetailsModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT d.*, h.Notes
            FROM DocumentDetails d
            OUTER APPLY (
                SELECT TOP 1 Notes
                FROM DocumentStatusHistories
                WHERE DocumentDetailId = d.Id
                ORDER BY CreatedOn DESC
            ) h
            WHERE d.CreatedBy = @CreatedBy AND d.ApproveStatus = 'rejected'";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CreatedBy", createdBy ?? (object)DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var doc = new DocumentDetailsModel
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                DocumentId = reader.GetInt32(reader.GetOrdinal("DocumentId")),
                                OriginalFileName = reader["OriginalFileName"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Description = reader["Description"].ToString(),
                                VersionNumber = reader.GetInt32(reader.GetOrdinal("VersionNumber")),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
                                CreatedFrom = reader["CreatedFrom"] as string,
                                LastUpdateBy = reader["LastUpdateBy"] as string,
                                LastUpdateOn = reader["LastUpdateOn"] as DateTime?,
                                LastUpdateFrom = reader["LastUpdateFrom"] as string,
                                ApproveStatus = reader["ApproveStatus"].ToString(),
                                IsArchive = reader["IsArchive"] is bool b ? b :
                                    reader["IsArchive"] is string s ? s == "True" || s == "true" || s == "1" :
                                    Convert.ToBoolean(reader["IsArchive"]),
                                ParentDocumentId = reader["ParentDocumentId"] as int?,
                            };

                            var notesOrdinal = reader.GetOrdinal("Notes");
                            if (!reader.IsDBNull(notesOrdinal))
                            {
                                doc.Notes = reader.GetString(notesOrdinal);
                            }

                            result.Add(doc);
                        }
                    }
                }
            }
            return result;
        }


        // for admin to show recent 5 documents 
        public List<DocumentDetailsModel> GetRecentApprovedOrPendingDocuments(int count = 5)
        {
            var result = new List<DocumentDetailsModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = $@"
                SELECT TOP (@Count) *
                FROM DocumentDetails
                WHERE ApproveStatus = 'approved' OR ApproveStatus = 'pending'
                ORDER BY CreatedOn DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Count", count);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var doc = new DocumentDetailsModel
                            {
                                Id = (int)reader["Id"],
                                DocumentId = (int)reader["DocumentId"],
                                OriginalFileName = reader["OriginalFileName"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Description = reader["Description"].ToString(),
                                VersionNumber = (int)reader["VersionNumber"],
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                ApproveStatus = reader["ApproveStatus"].ToString(),
                                IsArchive = Convert.ToInt32(reader["IsArchive"]) == 1,
                                ParentDocumentId = reader["ParentDocumentId"] == DBNull.Value ? (int?)null : (int)reader["ParentDocumentId"],
                                Tags = new List<string>()
                            };
                            result.Add(doc);
                        }
                    }
                }
            }
            return result;
        }


        // for user show his Recent 5 documents.
        public List<DocumentDetailsModel> GetRecentApprovedOrPendingDocumentsUser(string userName, int count = 5)
        {
            var result = new List<DocumentDetailsModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
            SELECT TOP (@Count) *
            FROM DocumentDetails
            WHERE (ApproveStatus = 'approved' OR ApproveStatus = 'pending')
              AND CreatedBy = @UserName
            ORDER BY CreatedOn DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Count", count);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var doc = new DocumentDetailsModel
                            {
                                Id = (int)reader["Id"],
                                DocumentId = (int)reader["DocumentId"],
                                OriginalFileName = reader["OriginalFileName"].ToString(),
                                FileName = reader["FileName"].ToString(),
                                Description = reader["Description"].ToString(),
                                VersionNumber = (int)reader["VersionNumber"],
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                ApproveStatus = reader["ApproveStatus"].ToString(),
                                IsArchive = Convert.ToInt32(reader["IsArchive"]) == 1,
                                ParentDocumentId = reader["ParentDocumentId"] == DBNull.Value ? (int?)null : (int)reader["ParentDocumentId"],
                                Tags = new List<string>()
                            };
                            result.Add(doc);
                        }
                    }
                }
            }
            return result;
        }

        // Monthly Pending Count
        public List<(int Year, int Month, int Count)> GetMonthlyPendingCount()
        {
            var result = new List<(int Year, int Month, int Count)>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                SELECT YEAR(CreatedOn) AS Year, MONTH(CreatedOn) AS Month, COUNT(*) AS Count
                FROM DocumentDetails
                WHERE ApproveStatus = 'pending'
                GROUP BY YEAR(CreatedOn), MONTH(CreatedOn)
                ORDER BY Year DESC, Month DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add((
                                reader.GetInt32(reader.GetOrdinal("Year")),
                                reader.GetInt32(reader.GetOrdinal("Month")),
                                reader.GetInt32(reader.GetOrdinal("Count"))
                            ));
                        }
                    }
                }
            }
            return result;
        }


        // Monthly Approved Count
        public List<(int Year, int Month, int Count)> GetMonthlyApprovedCount()
        {
            var result = new List<(int Year, int Month, int Count)>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                SELECT YEAR(CreatedOn) AS Year, MONTH(CreatedOn) AS Month, COUNT(*) AS Count
                FROM DocumentDetails
                WHERE ApproveStatus = 'approved'
                GROUP BY YEAR(CreatedOn), MONTH(CreatedOn)
                ORDER BY Year DESC, Month DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add((
                                reader.GetInt32(reader.GetOrdinal("Year")),
                                reader.GetInt32(reader.GetOrdinal("Month")),
                                reader.GetInt32(reader.GetOrdinal("Count"))
                            ));
                        }
                    }
                }
            }
            return result;
        }


        // Monthly Rejected Count
        public List<(int Year, int Month, int Count)> GetMonthlyRejectedCount()
        {
            var result = new List<(int Year, int Month, int Count)>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                SELECT YEAR(CreatedOn) AS Year, MONTH(CreatedOn) AS Month, COUNT(*) AS Count
                FROM DocumentDetails
                WHERE ApproveStatus = 'rejected'
                GROUP BY YEAR(CreatedOn), MONTH(CreatedOn)
                ORDER BY Year DESC, Month DESC";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add((
                                reader.GetInt32(reader.GetOrdinal("Year")),
                                reader.GetInt32(reader.GetOrdinal("Month")),
                                reader.GetInt32(reader.GetOrdinal("Count"))
                            ));
                        }
                    }
                }
            }
            return result;
        }



        public TagModel CreateTag(string tagName)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // First check if tag already exists
                string checkSql = "SELECT Id, Name FROM Tags WHERE Name = @Name";
                using (var checkCmd = new SqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Name", tagName.Trim());
                    using (var reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TagModel
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString()
                            };
                        }
                    }
                }

                // If tag doesn't exist, create it
                string insertSql = "INSERT INTO Tags (Name) VALUES (@Name); SELECT SCOPE_IDENTITY();";
                using (var insertCmd = new SqlCommand(insertSql, conn))
                {
                    insertCmd.Parameters.AddWithValue("@Name", tagName.Trim());
                    int newTagId = Convert.ToInt32(insertCmd.ExecuteScalar());

                    return new TagModel
                    {
                        Id = newTagId,
                        Name = tagName.Trim()
                    };
                }
            }
        }
    }
}
