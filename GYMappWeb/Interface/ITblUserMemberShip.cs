using GYMappWeb.Helper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblUser;
using GYMappWeb.ViewModels.TblUserMemberShip;

namespace GYMappWeb.Interface
{
    public interface ITblUserMemberShip
    {
        Task<PagedResult<GetWithPaginationTblUserMemberShipViewModel>> GetAllUserMembershipsAsync(UserParameters userParameters, int gymBranchId);
        Task UpdateExpiredMembershipsAsync(int gymBranchId);
        Task<bool> AddMembershipAsync(SaveTblUserMemberShipViewModel model, string createdById, int gymBranchId);
        Task<bool> DeleteMembershipAsync(int id, int gymBranchId);
        Task<bool> DeleteFreezesAsync(int userMemberShipId, int gymBranchId);
        Task<bool> HasActiveMembershipAsync(int userId, int gymBranchId);
        Task<TblUserMemberShip> GetMembershipByIdAsync(int id, int gymBranchId);
    }
}