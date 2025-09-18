using DMS_Final.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DMS_Final.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly string _connectionString;

        public NotificationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void InsertNotification(NotificationModel notification, SqlConnection conn, SqlTransaction transaction)
        {
            string sql = @"
                INSERT INTO Notifications (TargetUserId, ActorUserId, Message, DocumentId, DocumentDetailId, EventType, IsRead, CreatedOn)
                VALUES (@TargetUserId, @ActorUserId, @Message, @DocumentId, @DocumentDetailId, @EventType, @IsRead, @CreatedOn);";

            using (var cmd = new SqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@TargetUserId", notification.TargetUserId);
                cmd.Parameters.AddWithValue("@ActorUserId", notification.ActorUserId);
                cmd.Parameters.AddWithValue("@Message", notification.Message);
                cmd.Parameters.AddWithValue("@DocumentId", (object)notification.DocumentId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DocumentDetailId", (object)notification.DocumentDetailId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EventType", notification.EventType);
                cmd.Parameters.AddWithValue("@IsRead", notification.IsRead);
                cmd.Parameters.AddWithValue("@CreatedOn", notification.CreatedOn);
                cmd.ExecuteNonQuery();
            }
        }

        public int GetUnreadNotificationCountForUser(int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM Notifications WHERE TargetUserId = @UserId AND IsRead = 0;";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        public List<NotificationModel> GetRecentNotifications(int userId, int take)
        {
            var notifications = new List<NotificationModel>();
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT TOP (@Take) Id, TargetUserId, ActorUserId, Message, DocumentId, DocumentDetailId, EventType, IsRead, CreatedOn
                    FROM Notifications
                    WHERE TargetUserId = @UserId
                    ORDER BY CreatedOn DESC;";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Take", take);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notifications.Add(new NotificationModel
                            {
                                Id = (int)reader["Id"],
                                TargetUserId = (int)reader["TargetUserId"],
                                ActorUserId = (int)reader["ActorUserId"],
                                Message = reader["Message"].ToString(),
                                DocumentId = reader["DocumentId"] as int?,
                                DocumentDetailId = reader["DocumentDetailId"] as int?,
                                EventType = reader["EventType"].ToString(),
                                IsRead = (bool)reader["IsRead"],
                                CreatedOn = (DateTime)reader["CreatedOn"]
                            });
                        }
                    }
                }
            }
            return notifications;
        }

        public void MarkNotificationAsRead(int notificationId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Notifications SET IsRead = 1 WHERE Id = @Id;";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", notificationId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void MarkAllAsRead(int userId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE Notifications SET IsRead = 1 WHERE TargetUserId = @UserId AND IsRead = 0;";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
