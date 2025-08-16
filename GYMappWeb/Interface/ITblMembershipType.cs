using GYMappWeb.Helper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblMemberShipFreeze;
using GYMappWeb.ViewModels.TblMemberShipType;

namespace GYMappWeb.Interface
{
    public interface ITblMembershipType
    {
        Task<PagedResult<GetWithPaginationTblMemberShipTypeViewModel>> GetAllMembershipTypesAsync(UserParameters userParameters);
        Task<bool> AddMembershipTypeAsync(SaveTblMemberShipTypeViewModel model, string createdById);
        Task<bool> UpdateMembershipTypeAsync(SaveTblMemberShipTypeViewModel model, int id, string updatedById);
        Task<bool> DeleteMembershipTypeAsync(int id);
        Task<bool> HasRelatedMembershipsAsync(int membershipTypeId);
        Task<SaveTblMemberShipTypeViewModel> GetMembershipTypeDetailsAsync(int id);
    }
}
