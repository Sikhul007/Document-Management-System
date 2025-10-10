using DMS.Models;

namespace DMS.Repository.Admin
{
    public interface IAdminRepository
    {
        object GetAllUsers(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection);
        UserModel GetUserById(int id);
        void CreateUser(UserModel user);
        void UpdateUser(UserModel user);
        void DeleteUser(int id);

        int GetDocumentDetailsCount();
        int GetPendingDocumentDetailsCount();
        int GetApprovedDocumentDetailsCount();
        int GetActiveUserCount();

        List<(string UserName, int Approved, int Pending, int Rejected)> GetUserDocumentStats();
    }
}
