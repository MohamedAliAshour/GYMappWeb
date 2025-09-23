using GYMappWeb.Helper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblMemberShipFreeze;
using GYMappWeb.ViewModels.TblMemberShipType;

namespace GYMappWeb.Interface
{
    public interface ITblMembershipType
    {
        Task<PagedResult<GetWithPaginationTblMemberShipTypeViewModel>> GetAllMembershipTypesAsync(UserParameters userParameters, int gymBranchId);
        Task<bool> AddMembershipTypeAsync(SaveTblMemberShipTypeViewModel model, string createdById, int gymBranchId);
        Task<bool> UpdateMembershipTypeAsync(SaveTblMemberShipTypeViewModel model, int id, string updatedById, int gymBranchId);
        Task<bool> DeleteMembershipTypeAsync(int id, int gymBranchId);
        Task<bool> ToggleMembershipTypeStatusAsync(int id, int gymBranchId);
        Task<bool> HasRelatedMembershipsAsync(int membershipTypeId, int gymBranchId);
        Task<SaveTblMemberShipTypeViewModel> GetMembershipTypeDetailsAsync(int id, int gymBranchId);
    }
}