using DMS.Models;

namespace DMS.Services.User
{
    public interface IUserService
    {
        UserModel Authenticate(string username, string password);
        bool ChangePassword(int userId, string currentPassword, string newPassword);


        int GetRejectedDocumentDetailsCountUser(string userName);
        int GetPendingDocumentDetailsCountUser(string userName);
        int GetApprovedDocumentDetailsCountUser(string userName);




        public void SetLastLoginTime(int userId);
        void SetLastLogoutTime(int userId);
    }
}

