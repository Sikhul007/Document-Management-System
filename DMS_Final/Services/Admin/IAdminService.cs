using DMS_Final.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace DMS_Final.Services.Admin
{
    public interface IAdminService
    {
        void CreateUser(UserModel user);
        List<UserModel> GetAllUsers();
        void UpdateUser(UserModel user);
        void DeleteUser(int id);


        int GetDocumentDetailsCount();
        int GetPendingDocumentDetailsCount();
        int GetApprovedDocumentDetailsCount();
        int GetActiveUserCount();
    }
}
