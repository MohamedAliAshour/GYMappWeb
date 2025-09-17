using GYMappWeb.Helper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblOffer;
using GYMappWeb.ViewModels.TblUser;

namespace GYMappWeb.Interface
{
    public interface ITblUser
    {
        Task<PagedResult<GetWithPaginationTblUserViewModel>> GetWithPaginations(UserParameters userParam);
        Task<List<GetWithPaginationTblUserViewModel>> GetAllUsersAsync();
        Task<bool> Add(SaveTblUserViewModel model, string createdById);
        Task<bool> Update(SaveTblUserViewModel model, int id, string updatedById);
        Task<bool> Delete(int id);
        Task<bool> DeleteRelatedRecords(int id);

        Task<bool> CheckNameExist(string name);
        Task<bool> CheckPhoneExist(string phone);
        Task<int> GetNextUserCode();
        TblUser GetById(int id);
        SaveTblUserViewModel GetDetailsById(int id);
        Task<GetWithPaginationTblUserViewModel> GetUserDetailsAsync(int id);
    }
}
