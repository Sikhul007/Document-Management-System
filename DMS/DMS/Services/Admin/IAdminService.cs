using DMS.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace DMS.Services.Admin
{
    public interface IAdminService
    {
        void CreateUser(UserModel user);
        
        object GetAllUsers(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection);
        UserModel GetUserById(int id);
        void UpdateUser(UserModel user);
        void DeleteUser(int id);


        int GetDocumentDetailsCount();
        int GetPendingDocumentDetailsCount();
        int GetApprovedDocumentDetailsCount();
        int GetActiveUserCount();

        List<(string UserName, int Approved, int Pending, int Rejected)> GetUserDocumentStats();
    }
}
