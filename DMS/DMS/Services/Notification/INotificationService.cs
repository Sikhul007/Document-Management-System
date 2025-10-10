using DMS.Models;
using System.Collections.Generic;

namespace DMS.Services
{
    public interface INotificationService
    {
        void CreateNotification(int targetUserId, int actorUserId, string eventType, string message, int? documentId = null, int? documentDetailId = null);
        int GetUnreadNotificationCount(int userId);
        List<NotificationModel> GetRecentNotifications(int userId, int take);
        void MarkAsRead(int notificationId);
        void MarkAllAsRead(int userId);
    }
}