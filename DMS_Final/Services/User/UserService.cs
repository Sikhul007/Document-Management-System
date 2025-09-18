using DMS_Final.Models;
using DMS_Final.Repository.User;

namespace DMS_Final.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public UserModel Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = _userRepository.ValidateUser(username, password);

            if (user == null || !user.IsActive)
                return null;

            return user;
        }

        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return false;

            return _userRepository.ChangePassword(userId, currentPassword, newPassword);
        }

        public int GetApprovedDocumentDetailsCountUser(string userName)
        {
            return _userRepository.GetApprovedDocumentDetailsCountUser(userName);
        }

        public int GetPendingDocumentDetailsCountUser(string userName)
        {
            return _userRepository.GetPendingDocumentDetailsCountUser(userName);
        }

        public int GetRejectedDocumentDetailsCountUser(string userName)
        {
            return _userRepository.GetRejectedDocumentDetailsCountUser(userName);
        }



        public void SetLastLoginTime(int userId)
        {
            _userRepository.UpdateLastLoginTime(userId);
        }

        public void SetLastLogoutTime(int userId)
        {
            _userRepository.UpdateLastLogoutTime(userId);
        }

    }
}
