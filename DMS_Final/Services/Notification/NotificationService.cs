using DMS_Final.Models;
using DMS_Final.Repository;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DMS_Final.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly string _connectionString;

        public NotificationService(INotificationRepository notificationRepository, IConfiguration configuration)
        {
            _notificationRepository = notificationRepository;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public void CreateNotification(int targetUserId, int actorUserId, string eventType, string message, int? documentId = null, int? documentDetailId = null)
        {
            var notification = new NotificationModel
            {
                TargetUserId = targetUserId,
                ActorUserId = actorUserId,
                EventType = eventType,
                Message = message,
                DocumentId = documentId,
                DocumentDetailId = documentDetailId,
                IsRead = false,
                CreatedOn = DateTime.Now
            };

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        _notificationRepository.InsertNotification(notification, conn, transaction);
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

        public int GetUnreadNotificationCount(int userId) =>
            _notificationRepository.GetUnreadNotificationCountForUser(userId);

        public List<NotificationModel> GetRecentNotifications(int userId, int take) =>
            _notificationRepository.GetRecentNotifications(userId, take);

        public void MarkAsRead(int notificationId) =>
            _notificationRepository.MarkNotificationAsRead(notificationId);

        public void MarkAllAsRead(int userId) =>
            _notificationRepository.MarkAllAsRead(userId);
    }
}
