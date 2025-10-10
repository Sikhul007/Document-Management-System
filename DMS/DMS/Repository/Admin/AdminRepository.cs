using DMS.Models;
using System.Data;
using System.Data.SqlClient;

namespace DMS.Repository.Admin
{
    public class AdminRepository : IAdminRepository
    {
        private readonly string _connectionString;

        public AdminRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public void CreateUser(UserModel user)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO Users
                        (Name, UserName, Email, Roles, Password, IsActive, CreatedBy, CreatedOn, LastUpdateBy, LastUpdateOn)
                        VALUES
                        (@Name, @UserName, @Email, @Roles, @Password, @IsActive, @CreatedBy, @CreatedOn, @LastUpdateBy, @LastUpdateOn)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@UserName", user.UserName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Roles", user.Roles);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                cmd.Parameters.AddWithValue("@CreatedBy", user.CreatedBy);
                cmd.Parameters.AddWithValue("@CreatedOn", user.CreatedOn);
                cmd.Parameters.AddWithValue("@LastUpdateBy", user.LastUpdateBy ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LastUpdateOn", user.LastUpdateOn ?? (object)DBNull.Value);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    // SQL error 2627 = Violation of UNIQUE KEY constraint
                    // SQL error 2601 = Cannot insert duplicate key row
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        throw new Exception("Username or Email already exists.");
                    }
                    else
                    {
                        throw; // rethrow any other SQL error
                    }
                }
            }
        }


        public void DeleteUser(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string deleteNotificationsQuery = @"
                    DELETE FROM Notifications
                    WHERE TargetUserId = @Id OR ActorUserId = @Id";

                        using (SqlCommand cmdNotifications = new SqlCommand(deleteNotificationsQuery, conn, transaction))
                        {
                            cmdNotifications.Parameters.AddWithValue("@Id", id);
                            cmdNotifications.ExecuteNonQuery();
                        }

                        string deleteUserQuery = "DELETE FROM Users WHERE Id = @Id";
                        using (SqlCommand cmdUser = new SqlCommand(deleteUserQuery, conn, transaction))
                        {
                            cmdUser.Parameters.AddWithValue("@Id", id);
                            cmdUser.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Failed to delete user and related data: " + ex.Message, ex);
                    }
                }
            }
        }


        public object GetAllUsers(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection)
        {
            List<UserModel> users = new List<UserModel>();
            int totalRecords = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Get total count with search filter
                string countQuery = "SELECT COUNT(*) FROM Users WHERE 1=1";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    countQuery += " AND (Name LIKE @SearchTerm OR UserName LIKE @SearchTerm OR Email LIKE @SearchTerm OR Roles LIKE @SearchTerm)";
                }

                SqlCommand countCmd = new SqlCommand(countQuery, conn);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    countCmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                }
                totalRecords = (int)countCmd.ExecuteScalar();

                // Build main query with pagination and sorting
                string query = "SELECT * FROM Users WHERE 1=1";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " AND (Name LIKE @SearchTerm OR UserName LIKE @SearchTerm OR Email LIKE @SearchTerm OR Roles LIKE @SearchTerm)";
                }

                // Add sorting
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    string direction = sortDirection.ToLower() == "desc" ? "DESC" : "ASC";

                    // Validate sort column to prevent SQL injection
                    var validColumns = new[] { "Name", "UserName", "Email", "Roles", "IsActive" };
                    if (validColumns.Contains(sortColumn))
                    {
                        query += $" ORDER BY {sortColumn} {direction}";
                    }
                }
                else
                {
                    query += " ORDER BY Id ASC"; // Default sorting
                }

                // Add pagination
                query += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                SqlCommand cmd = new SqlCommand(query, conn);
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                }
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new UserModel()
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        UserName = reader["UserName"].ToString(),
                        Email = reader["Email"].ToString(),
                        Name = reader["Name"].ToString(),
                        Roles = reader["Roles"].ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        CreatedBy = reader["CreatedBy"].ToString(),
                        CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                        LastUpdateBy = reader["LastUpdateBy"] != DBNull.Value ? reader["LastUpdateBy"].ToString() : null,
                        LastUpdateOn = reader["LastUpdateOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["LastUpdateOn"])
                    });
                }
            }

            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            return new
            {
                users = users,
                totalRecords = totalRecords,
                totalPages = totalPages,
                currentPage = page
            };
        }

        public UserModel GetUserById(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Users WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new UserModel
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        UserName = reader["UserName"].ToString(),
                        Email = reader["Email"].ToString(),
                        Name = reader["Name"].ToString(),
                        Roles = reader["Roles"].ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        CreatedBy = reader["CreatedBy"].ToString(),
                        CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                        LastUpdateBy = reader["LastUpdateBy"] != DBNull.Value ? reader["LastUpdateBy"].ToString() : null,
                        LastUpdateOn = reader["LastUpdateOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["LastUpdateOn"])
                    };
                }
            }

            return null;
        }


        public void UpdateUser(UserModel user)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE Users
                                 SET Name = @Name,
                                     UserName = @UserName,
                                     Email = @Email,
                                     Roles = @Roles,
                                     IsActive = @IsActive,
                                     LastUpdateBy = @LastUpdateBy,
                                     LastUpdateOn = @LastUpdateOn
                                 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@UserName", user.UserName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Roles", user.Roles);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                cmd.Parameters.AddWithValue("@LastUpdateBy", user.LastUpdateBy ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LastUpdateOn", user.LastUpdateOn ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", user.Id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }



        public int GetDocumentDetailsCount()
        {
           
            return GetCount("ApproveStatus IN ('Approved', 'Pending' , 'rejected')");
        }

        public int GetPendingDocumentDetailsCount()
        {
            
            return GetCount("ApproveStatus = 'Pending'");
        }

        public int GetApprovedDocumentDetailsCount()
        {

            return GetCount("ApproveStatus = 'approved'");
        }

        private int GetCount(string whereClause)
        {
            int count = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                
                string query = $"SELECT COUNT(*) FROM DocumentDetails WHERE {whereClause};";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    
                    object result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        count = Convert.ToInt32(result);
                    }
                }
            }
            return count;
        }


        public int GetActiveUserCount()
        {
            int count = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM Users WHERE IsActive = 1;";
                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    count = (int)cmd.ExecuteScalar();
                }
            }
            return count;
        }


        public List<(string UserName, int Approved, int Pending, int Rejected)> GetUserDocumentStats()
        {
            var list = new List<(string, int, int, int)>();

            using (var conn = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT
                CreatedBy,
                SUM(CASE WHEN LOWER(ApproveStatus) = 'approved' THEN 1 ELSE 0 END) AS ApprovedCount,
                SUM(CASE WHEN LOWER(ApproveStatus) = 'pending'  THEN 1 ELSE 0 END) AS PendingCount,
                SUM(CASE WHEN LOWER(ApproveStatus) = 'rejected' THEN 1 ELSE 0 END) AS RejectedCount
            FROM DocumentDetails
            GROUP BY CreatedBy;";

                using (var cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add((
                                reader["CreatedBy"].ToString(),
                                Convert.ToInt32(reader["ApprovedCount"]),
                                Convert.ToInt32(reader["PendingCount"]),
                                Convert.ToInt32(reader["RejectedCount"])
                            ));
                        }
                    }
                }
            }

            return list;
        }

    }
}
