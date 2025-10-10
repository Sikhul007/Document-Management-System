using DMS.Models;
using DMS.Repository.Admin;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace DMS.Services.Admin
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

        public object GetAllUsers(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection)
        {
            return _adminRepository.GetAllUsers(page, pageSize, searchTerm, sortColumn, sortDirection);
        }

        public UserModel GetUserById(int id)
        {
            return _adminRepository.GetUserById(id);
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


        public List<(string UserName, int Approved, int Pending, int Rejected)> GetUserDocumentStats()
        {
            return _adminRepository.GetUserDocumentStats();
        }

    }
}
