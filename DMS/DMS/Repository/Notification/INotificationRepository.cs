using DMS.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DMS.Repository
{
    public interface INotificationRepository
    {
        void InsertNotification(NotificationModel notification, SqlConnection conn, SqlTransaction transaction);
        int GetUnreadNotificationCountForUser(int userId);
        List<NotificationModel> GetRecentNotifications(int userId, int take);
        void MarkNotificationAsRead(int notificationId);
        void MarkAllAsRead(int userId);
    }
}
