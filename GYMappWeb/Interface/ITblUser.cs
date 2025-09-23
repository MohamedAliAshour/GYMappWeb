using GYMappWeb.Helper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblOffer;
using GYMappWeb.ViewModels.TblUser;

namespace GYMappWeb.Interface
{
    public interface ITblUser
    {
        Task<PagedResult<GetWithPaginationTblUserViewModel>> GetWithPaginations(UserParameters userParam, int gymBranchId);

        Task<bool> Add(SaveTblUserViewModel model, string createdById, int gymBranchId);
        Task<bool> Update(SaveTblUserViewModel model, int id, string updatedById);
        Task<bool> Delete(int id);
        Task<bool> DeleteRelatedRecords(int id);
        Task<bool> CheckPhoneExist(string phone, int gymBranchId);
        Task<int> GetNextUserCode(int gymBranchId);
        TblUser GetById(int id, int gymBranchId);
        SaveTblUserViewModel GetDetailsById(int id, int gymBranchId);
        Task<GetWithPaginationTblUserViewModel> GetUserDetailsAsync(int id, int gymBranchId);
    }
}
