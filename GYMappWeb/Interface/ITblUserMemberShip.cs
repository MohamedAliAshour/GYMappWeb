using GYMappWeb.Helper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblUser;
using GYMappWeb.ViewModels.TblUserMemberShip;

namespace GYMappWeb.Interface
{
    public interface ITblUserMemberShip
    {
        Task<PagedResult<GetWithPaginationTblUserMemberShipViewModel>> GetAllUserMembershipsAsync(UserParameters userParameters);
        Task UpdateExpiredMembershipsAsync();
        Task<bool> AddMembershipAsync(SaveTblUserMemberShipViewModel model, string createdById);
        Task<bool> DeleteMembershipAsync(int id);
        Task<bool> DeleteFreezesAsync(int userMemberShipId);
        Task<bool> HasActiveMembershipAsync(int userId);
        Task<TblUserMemberShip> GetMembershipByIdAsync(int id);
    }
}
