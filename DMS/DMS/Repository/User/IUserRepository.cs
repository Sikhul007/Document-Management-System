using DMS.Models;

namespace DMS.Repository.User
{
    public interface IUserRepository
    {
        UserModel ValidateUser(string username, string password);
        bool ChangePassword(int userId, string currentPassword, string newPassword);




        int GetRejectedDocumentDetailsCountUser(string userName);
        int GetPendingDocumentDetailsCountUser(string userName);
        int GetApprovedDocumentDetailsCountUser(string userName);


        void UpdateLastLoginTime(int userId);
        void UpdateLastLogoutTime(int userId);


        List<UserModel> GetUsersByRole(string roleName);
        int GetUserIdByUsername(string username);
    }
}
