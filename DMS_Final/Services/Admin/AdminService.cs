using DMS_Final.Models;
using DMS_Final.Repository.Admin;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace DMS_Final.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }
        public void CreateUser(UserModel user)
        {
            user.IsActive = true; 
            user.CreatedOn = DateTime.Now; 
            user.LastUpdateBy = null; 
            user.LastUpdateOn = null; 
            _adminRepository.CreateUser(user);
        }

        public void DeleteUser(int id)
        {
            _adminRepository.DeleteUser(id);
        }
        // Hello

        public List<UserModel> GetAllUsers()
        {
            return _adminRepository.GetAllUsers();
        }

        public void UpdateUser(UserModel user)
        {
            _adminRepository.UpdateUser(user);
        }


        public int GetDocumentDetailsCount()
        {
            return _adminRepository.GetDocumentDetailsCount();
        }

        public int GetPendingDocumentDetailsCount()
        {
            return _adminRepository.GetPendingDocumentDetailsCount();
        }

        public int GetApprovedDocumentDetailsCount()
        {
            return _adminRepository.GetApprovedDocumentDetailsCount();
        }

        public int GetActiveUserCount()
        {
            return _adminRepository.GetActiveUserCount();
        }

    }
}
