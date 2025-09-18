using DMS_Final.Models;

namespace DMS_Final.Repository.Admin
{
    public interface IAdminRepository
    {
        List<UserModel> GetAllUsers();
        //UserModel GetUserById(int id);
        void CreateUser(UserModel user);
        void UpdateUser(UserModel user);
        void DeleteUser(int id);

        int GetDocumentDetailsCount();
        int GetPendingDocumentDetailsCount();
        int GetApprovedDocumentDetailsCount();
        int GetActiveUserCount();
    }
}
