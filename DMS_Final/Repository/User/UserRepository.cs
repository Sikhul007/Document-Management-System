using DMS_Final.Models;
using System.Data.SqlClient;
using DMS_Final.Repository.User;

namespace DMS_Final.Repository.User
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public UserModel ValidateUser(string username, string password)
        {
            UserModel user = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT * FROM Users 
                               WHERE UserName = @UserName COLLATE SQL_Latin1_General_CP1_CS_AS
                               AND Password = @Password 
                               AND IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        user = new UserModel
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            UserName = reader["UserName"].ToString(),
                            Name = reader["Name"].ToString(),
                            Email = reader["Email"].ToString(),
                            Roles = reader["Roles"].ToString(),
                            Password = reader["Password"].ToString(),
                            IsActive = Convert.ToBoolean(reader["IsActive"]),
                            CreatedBy = reader["CreatedBy"].ToString(),
                            CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                            LastUpdateBy = reader["LastUpdateBy"] != DBNull.Value ? reader["LastUpdateBy"].ToString() : null,
                            LastUpdateOn = reader["LastUpdateOn"] != DBNull.Value ? Convert.ToDateTime(reader["LastUpdateOn"]) : null
                        };
                    }
                }
            }

            return user;
        }

        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE Users 
                       SET Password = @NewPassword, LastUpdateOn = @LastUpdateOn 
                       WHERE Id = @UserId AND Password = @CurrentPassword";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@CurrentPassword", currentPassword);
                    cmd.Parameters.AddWithValue("@NewPassword", newPassword);
                    cmd.Parameters.AddWithValue("@LastUpdateOn", DateTime.Now);

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }




        public int GetRejectedDocumentDetailsCountUser(string userName)
        {
            return GetCountUser("ApproveStatus IN ('rejected')", userName);
        }

        public int GetPendingDocumentDetailsCountUser(string userName)
        {
            return GetCountUser("ApproveStatus = 'Pending'", userName);
        }

        public int GetApprovedDocumentDetailsCountUser(string userName)
        {
            return GetCountUser("ApproveStatus = 'Approved'", userName);
        }



        private int GetCountUser(string whereClause, string userName)
        {
            int count = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                string query = $"SELECT COUNT(*) FROM DocumentDetails WHERE {whereClause} AND CreatedBy = @UserName;";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", userName);
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













        public void UpdateLastLoginTime(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Users SET LastLoginTime = @LoginTime WHERE Id = @UserId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@LoginTime", DateTime.Now);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateLastLogoutTime(int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Users SET LastLogoutTime = @LogoutTime WHERE Id = @UserId";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@LogoutTime", DateTime.Now);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }








        public List<UserModel> GetUsersByRole(string roleName)
        {
            var users = new List<UserModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                // Note: I'm using "Roles" to match your UserModel
                string sql = "SELECT Id, Name, UserName, Email, Roles, IsActive, CreatedBy, CreatedOn, LastUpdateBy, LastUpdateOn, LastLoginTime, LastLogoutTime FROM Users WHERE Roles = @RoleName;";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@RoleName", roleName);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new UserModel
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString(),
                                UserName = reader["UserName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Roles = reader["Roles"].ToString(),
                                IsActive = (bool)reader["IsActive"],
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedOn = (DateTime)reader["CreatedOn"],
                                LastUpdateBy = reader["LastUpdateBy"] as string,
                                LastUpdateOn = reader["LastUpdateOn"] as DateTime?,
                                LastLoginTime = reader["LastLoginTime"] as DateTime?,
                                LastLogoutTime = reader["LastLogoutTime"] as DateTime?
                            });
                        }
                    }
                }
            }
            return users;
        }

        public int GetUserIdByUsername(string username)
        {
            int userId = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                // Note: I'm using "UserName" to match your UserModel
                string sql = "SELECT Id FROM Users WHERE UserName = @Username;";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        userId = Convert.ToInt32(result);
                    }
                }
            }
            return userId;
        }

    }
}

