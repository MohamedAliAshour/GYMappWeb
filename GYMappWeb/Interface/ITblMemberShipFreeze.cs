using GYMappWeb.Helper;
using GYMappWeb.Models;
using GYMappWeb.ViewModels.TblMemberShipFreeze;

namespace GYMappWeb.Interface
{
    public interface ITblMemberShipFreeze
    {
        Task<PagedResult<GetWithPaginationTblMemberShipFreezeViewModel>> GetAllFreezesAsync(UserParameters userParameters);
        Task<List<object>> GetFreezeRecordsAsync(int userMembershipId);
        Task<object> GetMembershipFreezeDetailsAsync(int userMembershipId);
        Task<bool> AddFreezeAsync(SaveTblMemberShipFreezeViewModel model, string createdById);
        Task<bool> DeleteFreezeAsync(int id);
        Task<List<object>> GetActiveMembershipsForDropdownAsync();
    }
}
