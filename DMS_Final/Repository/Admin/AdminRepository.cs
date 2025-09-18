using DMS_Final.Models;
using System.Data.SqlClient;

namespace DMS_Final.Repository.Admin
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
                                (Name, UserName, Email,Roles, Password, IsActive, CreatedBy, CreatedOn, LastUpdateBy, LastUpdateOn)
                                VALUES
                                 (@Name, @UserName, @Email,@Roles, @Password, @IsActive, @CreatedBy, @CreatedOn, @LastUpdateBy, @LastUpdateOn)";
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

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteUser(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Users WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<UserModel> GetAllUsers()
        {
            List<UserModel> users = new List<UserModel>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Users";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
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
            return users;
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


    }
}
