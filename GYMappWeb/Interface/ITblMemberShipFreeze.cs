using GYMappWeb.Helper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblMemberShipFreeze;

namespace GYMappWeb.Interface
{
    public interface ITblMemberShipFreeze
    {
        Task<PagedResult<GetWithPaginationTblMemberShipFreezeViewModel>> GetAllFreezesAsync(UserParameters userParameters, int gymBranchId);
        Task<List<object>> GetFreezeRecordsAsync(int userMembershipId, int gymBranchId);
        Task<object> GetMembershipFreezeDetailsAsync(int userMembershipId, int gymBranchId);
        Task<bool> HasDateOverlapAsync(int userMembershipId, DateTime startDate, DateTime endDate, int gymBranchId);
        Task<bool> AddFreezeAsync(SaveTblMemberShipFreezeViewModel model, string createdById, int gymBranchId);
        Task<bool> DeleteFreezeAsync(int id, int gymBranchId);
        Task<List<object>> GetActiveMembershipsForDropdownAsync(int gymBranchId);
    }
}